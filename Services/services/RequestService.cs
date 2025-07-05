using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using RestAPI.Enums;
using RestAPI.Helpers;
using RestAPI.Models;
using RestAPI.Models.SubModels;
using RestAPI.Repositories.interfaces;
using RestAPI.Services.interfaces;

namespace RestAPI.Services.services
{
    public class RequestService(
        IRequestRepository _requestRepository,
        IChildRepository _childRepository,
        IUserRepository _userRepository,
        IConsultationRepository _consultationRepository
    ) : IRequestService
    {
        public async Task<RequestModel> CreateRequest(
            RequestModel requestData,
            UserInfo requesterInfo
        )
        {
            try
            {
                if (Enum.Parse<RoleEnum>(requesterInfo.Role) == RoleEnum.User)
                {
                    requestData.MemberId = requesterInfo.UserId;
                }
                else
                {
                    throw new Exception("Only member can create a consultation request");
                }

                var checkChild = await _childRepository.GetByIdAsync(requestData.ChildId);
                if (checkChild == null)
                {
                    throw new Exception("Child not found");
                }

                if (checkChild.GuardianId != requesterInfo.UserId)
                {
                    throw new Exception(
                        "You are not authorized to request a consultation request for this child"
                    );
                }

                var checkDoctor = await _userRepository.GetByIdAsync(requestData.DoctorId);
                if (checkDoctor == null)
                {
                    throw new Exception("Doctor not found");
                }
                if (Enum.Parse<RoleEnum>(checkDoctor.Role) != RoleEnum.Doctor)
                {
                    throw new Exception("The requested doctor is not a doctor");
                }

                var request = await _requestRepository.CreateAsync(requestData);
                return request;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<RequestModel> DeleteRequest(string id, UserInfo requesterInfo)
        {
            try
            {
                var requesterRole = requesterInfo.Role;
                var requesterId = requesterInfo.UserId;

                if (Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.Doctor)
                {
                    throw new Exception("You are not authorized to delete this request");
                }

                var checkRequest = await _requestRepository.GetByIdAsync(id);
                if (checkRequest == null)
                {
                    throw new Exception("Request not found");
                }

                if (
                    Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.User
                    && checkRequest.MemberId != requesterId
                )
                {
                    throw new Exception("You are not authorized to delete this request");
                }

                var request = await _requestRepository.DeleteAsync(id);
                if (request == null)
                {
                    throw new Exception("Request not found");
                }
                return request;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        //admin view Only
        public async Task<PaginationResult<RequestModel>> GetAllRequests(
            QueryParams query,
            UserInfo requesterInfo,
            string? status
        )
        {
            try
            {
                var requests = await _requestRepository.GetAllRequestWithPagination(query, status);
                return requests;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<RequestModel> GetRequestById(string id, UserInfo requesterInfo)
        {
            try
            {
                var requesterRole = requesterInfo.Role;
                var requesterId = requesterInfo.UserId;

                var request = await _requestRepository.GetByIdAsync(
                    id,
                    new PopulationModel[]
                    {
                        new PopulationModel
                        {
                            LocalField = "memberId",
                            ForeignField = "_id",
                            Collection = "users",
                            As = "member",
                        },
                        new PopulationModel
                        {
                            LocalField = "doctorId",
                            ForeignField = "_id",
                            Collection = "users",
                            As = "doctor",
                        },
                        new PopulationModel
                        {
                            LocalField = "childId",
                            ForeignField = "_id",
                            Collection = "children",
                            As = "child",
                        },
                    }
                );
                if (request == null)
                {
                    throw new KeyNotFoundException("Request not found");
                }

                // Role-based access control
                if (Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.User)
                {
                    // Users can only see their own requests
                    if (request.MemberId != requesterId)
                    {
                        throw new KeyNotFoundException("Request not found");
                    }
                }
                else if (Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.Doctor)
                {
                    // Doctors can see any request assigned to them (regardless of status)
                    // This includes pending, accepted, rejected requests
                    if (request.DoctorId != requesterId)
                    {
                        throw new KeyNotFoundException("Request not found");
                    }
                }
                // Admins can see all requests (no additional check needed)

                return request;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<PaginationResult<RequestModel>> GetRequestsByDoctorId(
            string doctorId,
            QueryParams query,
            UserInfo requesterInfo
        )
        {
            try
            {
                var requesterRole = requesterInfo.Role;
                var requesterId = requesterInfo.UserId;

                if (Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.User)
                {
                    throw new Exception("You are not authorized to access this request");
                }

                if (
                    Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.Doctor
                    && requesterId != doctorId
                )
                {
                    throw new Exception("You are not authorized to access this request");
                }

                // Doctors can see all requests assigned to them (accepted, rejected, pending admin approval)
                // They need to track their decisions and ongoing consultations
                // If no status filter is provided, show all their assigned requests
                // If status filter is provided, respect the filter

                var requests = await _requestRepository.GetRequestsByDoctorId(doctorId, query);
                return requests;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<PaginationResult<RequestModel>> GetRequestsByMemberId(
            string memberId,
            QueryParams query,
            UserInfo requesterInfo,
            string? status
        )
        {
            try
            {
                var requesterRole = requesterInfo.Role;
                var requesterId = requesterInfo.UserId;
                if (Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.User && requesterId != memberId)
                {
                    throw new Exception("You are not authorized to access this request");
                }

                if (Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.Doctor)
                {
                    throw new Exception("You are not authorized to access this request");
                }

                var requests = await _requestRepository.GetRequestsByMemberId(
                    memberId,
                    query,
                    status
                );
                return requests;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<RequestModel> UpdateRequest(
            string id,
            RequestModel request,
            UserInfo requesterInfo
        )
        {
            try
            {
                var requesterRole = requesterInfo.Role;
                var requesterId = requesterInfo.UserId;

                var checkRequest = await _requestRepository.GetByIdAsync(id);
                if (checkRequest == null)
                {
                    throw new KeyNotFoundException("Request not found");
                }

                if (
                    Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.User
                    && requesterId != checkRequest.MemberId
                )
                {
                    throw new Exception("You are not authorized to update this request");
                }
                if (request.ChildId != null)
                {
                    var checkChild = await _childRepository.GetByIdAsync(request.ChildId);
                    if (checkChild == null)
                    {
                        throw new Exception("Child not found");
                    }
                    if (checkChild.GuardianId != requesterId)
                    {
                        throw new Exception("You are not authorized to update this request");
                    }

                    checkRequest.ChildId = request.ChildId;
                }
                if (request.DoctorId != null)
                {
                    var checkDoctor = await _userRepository.GetByIdAsync(request.DoctorId);
                    if (checkDoctor == null)
                    {
                        throw new Exception("Doctor not found");
                    }
                    if (Enum.Parse<RoleEnum>(checkDoctor.Role) != RoleEnum.Doctor)
                    {
                        throw new Exception("The requested doctor is not a doctor");
                    }
                    checkRequest.DoctorId = request.DoctorId;
                }
                if (request.Message != null)
                {
                    checkRequest.Message = request.Message;
                }

                var updatedrequest = await _requestRepository.UpdateAsync(id, checkRequest);
                if (updatedrequest == null)
                {
                    throw new Exception("Request not found");
                }
                return updatedrequest;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<RequestModel> UpdateRequestStatus(
            string id,
            RequestStatusEnum status,
            UserInfo requesterInfo
        )
        {
            try
            {
                var requesterRole = requesterInfo.Role;
                var requesterId = requesterInfo.UserId;

                var checkRequest = await _requestRepository.GetByIdAsync(id);
                if (checkRequest == null)
                {
                    throw new KeyNotFoundException("Request not found");
                }

                // Role-based status update permissions
                if (Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.User)
                {
                    // Users generally shouldn't update request status
                    throw new Exception("Users cannot update request status");
                }

                if (Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.Doctor)
                {
                    // Doctors can only accept/reject requests assigned to them
                    if (checkRequest.DoctorId != requesterId)
                    {
                        throw new Exception("You are not authorized to update this request");
                    }

                    // Doctors can only set Doctor_Accepted or Doctor_Rejected
                    if (
                        status != RequestStatusEnum.Doctor_Accepted
                        && status != RequestStatusEnum.Doctor_Rejected
                    )
                    {
                        throw new Exception("Doctors can only accept or reject requests");
                    }

                    // Doctors can only update requests that have been admin-approved
                    if (checkRequest.Status != RequestStatusEnum.Admin_Accepted)
                    {
                        throw new Exception(
                            "Can only accept/reject requests that have been admin-approved"
                        );
                    }
                }

                if (Enum.Parse<RoleEnum>(requesterRole) == RoleEnum.Admin)
                {
                    // Admins can set Admin_Accepted or Admin_Rejected
                    if (
                        status != RequestStatusEnum.Admin_Accepted
                        && status != RequestStatusEnum.Admin_Rejected
                    )
                    {
                        throw new Exception("Admins can only accept or reject requests");
                    }

                    // Admins can only update pending requests
                    if (
                        checkRequest.Status != RequestStatusEnum.Pending
                        && checkRequest.Status != RequestStatusEnum.Admin_Rejected
                    )
                    {
                        throw new Exception("Can only update pending and rejected requests");
                    }
                }

                checkRequest.Status = status;
                var updatedRequest = await _requestRepository.UpdateAsync(id, checkRequest);
                if (updatedRequest == null)
                {
                    throw new Exception("Request not found");
                }

                if (status == RequestStatusEnum.Doctor_Accepted)
                {
                    var consultation = new ConsultationModel
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        RequestId = id,
                        Status = ConsultationStatusEnum.Ongoing,
                    };

                    await _consultationRepository.CreateAsync(consultation);
                }
                return updatedRequest;
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}
