using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Models.SubModels;
using RestAPI.Repositories.interfaces;
using RestAPI.Services.interfaces;

namespace RestAPI.Services.services
{
    public class ConsultationMessageService(
        IConsultationMessageRepository _consultationMessageRepository,
        IUserRepository _userRepository,
        IConsultationRepository _consultationRepository
    ) : IConsultationMessageService
    {
        public async Task<ConsultationMessageModel> CreateConsultationMessage(
            ConsultationMessageModel message,
            UserInfo userInfo
        )
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userInfo.UserId);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found");
                }

                var consultation = await _consultationRepository.GetByIdAsync(
                    message.ConsultationId,
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
                    }
                );
                if (consultation == null)
                {
                    throw new KeyNotFoundException("Consultation not found");
                }
                if (
                    consultation.Request.DoctorId != userInfo.UserId
                    && consultation.Request.MemberId != userInfo.UserId
                    && userInfo.Role != "Admin"
                )
                {
                    throw new UnauthorizedAccessException(
                        "You are not authorized to create a consultation message for this consultation"
                    );
                }

                if (consultation.Status != Enums.ConsultationStatusEnum.Ongoing)
                {
                    throw new Exception("Consultation has ended");
                }

                message.SenderId = userInfo.UserId;

                var consultationMessage = await _consultationMessageRepository.CreateAsync(message);
                return consultationMessage;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<ConsultationMessageModel> DeleteConsultationMessage(
            string id,
            UserInfo userInfo
        )
        {
            try
            {
                try
                {
                    var consultationMessage = await _consultationMessageRepository.GetByIdAsync(id);
                    if (consultationMessage == null)
                    {
                        throw new KeyNotFoundException("Consultation message not found");
                    }

                    var consultation = await _consultationRepository.GetByIdAsync(
                        consultationMessage.ConsultationId
                    );

                    if (consultation == null)
                    {
                        throw new KeyNotFoundException("Consultation not found");
                    }

                    if (consultation.Status != Enums.ConsultationStatusEnum.Ongoing)
                    {
                        throw new Exception("Consultation has ended");
                    }

                    if (consultationMessage.SenderId != userInfo.UserId)
                    {
                        throw new UnauthorizedAccessException(
                            "You are not authorized to delete this consultation message"
                        );
                    }

                    await _consultationMessageRepository.DeleteAsync(id);
                    return consultationMessage;
                }
                catch (System.Exception)
                {
                    throw;
                }
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<ConsultationMessageModel> GetConsultationMessageById(string id)
        {
            try
            {
                var consultationMessage = await _consultationMessageRepository.GetByIdAsync(
                    id,
                    new PopulationModel[]
                    {
                        new PopulationModel
                        {
                            LocalField = "senderId",
                            ForeignField = "_id",
                            Collection = "users",
                            As = "sender",
                        },
                    }
                );
                if (consultationMessage == null)
                {
                    throw new KeyNotFoundException("Consultation message not found");
                }
                return consultationMessage;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<PaginationResult<ConsultationMessageModel>> GetConsultationMessages(
            string consultationId,
            QueryParams query
        )
        {
            try
            {
                var messages = await _consultationMessageRepository.GetConsultationMessages(
                    consultationId,
                    query
                );
                return messages;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<ConsultationMessageModel> UpdateConsultationMessage(
            string id,
            string message,
            UserInfo userInfo
        )
        {
            try
            {
                var consultationMessage = await _consultationMessageRepository.GetByIdAsync(id);
                if (consultationMessage == null)
                {
                    throw new KeyNotFoundException("Consultation message not found");
                }

                var consultation = await _consultationRepository.GetByIdAsync(
                    consultationMessage.ConsultationId
                );

                if (consultation == null)
                {
                    throw new KeyNotFoundException("Consultation not found");
                }

                if (consultation.Status != Enums.ConsultationStatusEnum.Ongoing)
                {
                    throw new Exception("Consultation has ended");
                }

                consultationMessage.Message = message;
                var updatedMessage = await _consultationMessageRepository.UpdateAsync(
                    id,
                    consultationMessage
                );
                if (updatedMessage == null)
                {
                    throw new KeyNotFoundException("Consultation message not found");
                }
                return updatedMessage;
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}
