using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestAPI.Enums;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Models.SubModels;
using RestAPI.Repositories.interfaces;
using RestAPI.Services.interfaces;

namespace RestAPI.Services.services
{
    public class ConsultationService(
        IConsultationRepository _consultationRepository,
        IRequestRepository _requestRepository
    ) : IConsultationService
    {
        public Task<PaginationResult<ConsultationModel>> GetConsultations(
            QueryParams query,
            string? status
        )
        {
            try
            {
                var consultations = _consultationRepository.GetConsultations(query, status);

                return consultations;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public Task<PaginationResult<ConsultationModel>> GetConsultationsByDoctorId(
            string requestId,
            QueryParams query,
            string? status
        )
        {
            try
            {
                var consultations = _consultationRepository.GetConsultationsByDoctorId(
                    requestId,
                    query,
                    status
                );

                return consultations;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public Task<PaginationResult<ConsultationModel>> GetConsultationsByMemberId(
            string requestId,
            QueryParams query,
            string? status
        )
        {
            try
            {
                var consultations = _consultationRepository.GetConsultationsByMemberId(
                    requestId,
                    query,
                    status
                );

                return consultations;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        //only user => no need role check
        public async Task<ConsultationModel> RateConsultationById(
            string id,
            int rating,
            UserInfo requesterInfo
        )
        {
            try
            {
                var checkConsultation = await _consultationRepository.GetByIdAsync(id);
                if (checkConsultation == null)
                {
                    throw new Exception("Consultation not found");
                }

                if (checkConsultation.Status != ConsultationStatusEnum.Completed)
                {
                    throw new Exception("Consultation must be completed before rating");
                }

                var request = await _requestRepository.GetByIdAsync(checkConsultation.RequestId);
                if (request == null)
                {
                    throw new Exception("Request not found");
                }

                if (Enum.Parse<RoleEnum>(requesterInfo.Role) == RoleEnum.User)
                {
                    if (request.MemberId != requesterInfo.UserId)
                    {
                        throw new Exception("You are not the member of this consultation");
                    }
                }

                var consultation = await _consultationRepository.RateConsultationById(id, rating);

                return consultation;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<ConsultationModel> UpdateConsultationStatus(
            string id,
            string status,
            UserInfo requesterInfo
        )
        {
            try
            {
                var checkConsultation = await _consultationRepository.GetByIdAsync(id);
                if (checkConsultation == null)
                {
                    throw new Exception("Consultation not found");
                }

                var requesterId = requesterInfo.UserId;

                var request = await _requestRepository.GetByIdAsync(checkConsultation.RequestId);
                if (request == null)
                {
                    throw new Exception("Request not found");
                }

                if (
                    Enum.Parse<RoleEnum>(requesterInfo.Role) == RoleEnum.User
                    && request.MemberId != requesterId
                )
                {
                    throw new Exception("You are not the owner of this request");
                }

                checkConsultation.Status = Enum.Parse<ConsultationStatusEnum>(status);

                var consultation = await _consultationRepository.UpdateAsync(id, checkConsultation);
                if (consultation == null)
                {
                    throw new Exception("Consultation not found");
                }
                return consultation;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<ConsultationModel> GetConsultationById(string id, UserInfo requesterInfo)
        {
            try
            {
                var checkConsultation = await _consultationRepository.GetByIdAsync(
                    id,
                    new PopulationModel[]
                    {
                        new PopulationModel
                        {
                            LocalField = "requestId",
                            ForeignField = "_id",
                            Collection = "requests",
                            As = "request",
                        },
                        new PopulationModel
                        {
                            LocalField = "request.doctorId",
                            ForeignField = "_id",
                            Collection = "users",
                            As = "request.doctor",
                        },
                        new PopulationModel
                        {
                            LocalField = "request.memberId",
                            ForeignField = "_id",
                            Collection = "users",
                            As = "request.member",
                        },
                        new PopulationModel
                        {
                            LocalField = "request.childId",
                            ForeignField = "_id",
                            Collection = "children",
                            As = "request.child",
                        },
                    }
                );
                if (checkConsultation == null)
                {
                    throw new Exception("Consultation not found");
                }

                var requesterRole = requesterInfo.Role;
                var requesterId = requesterInfo.UserId;

                var request = await _requestRepository.GetByIdAsync(checkConsultation.RequestId);
                if (request == null)
                {
                    throw new Exception("Request not found");
                }

                if (Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.User)
                {
                    if (request.MemberId != requesterId)
                    {
                        throw new Exception("You are not the owner of this request");
                    }
                }

                if (Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.Doctor)
                {
                    if (request.DoctorId != requesterId)
                    {
                        throw new Exception("You are not the doctor assigned to this request");
                    }
                }

                return checkConsultation;
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}
