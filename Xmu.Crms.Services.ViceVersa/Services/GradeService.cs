using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xmu.Crms.Services.ViceVersa.Daos;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;

namespace Xmu.Crms.Services.ViceVersa.Services
{
    internal class GradeService : IGradeService
    {
        private readonly IGradeDao _iGradeDao;
        private readonly ISeminarGroupService _iSeminarGroupService;
        private readonly ISeminarService _iSeminarService;
        private readonly ITopicService _iTopicService;
        private readonly IUserService _iUserService;

        public GradeService(IGradeDao iGradeDao, IUserService iUserService, ITopicService iTopicService,
            ISeminarGroupService iSeminarGroupService, ISeminarService iSeminarService)
        {
            _iGradeDao = iGradeDao;
            _iUserService = iUserService;
            _iTopicService = iTopicService;
            _iSeminarGroupService = iSeminarGroupService;
            _iSeminarService = iSeminarService;
        }

        public async Task DeleteStudentScoreGroupByTopicIdAsync(long topicId)
        {
            await _iGradeDao.DeleteStudentScoreGroupByTopicIdAsync(topicId);
        }

        public Task<SeminarGroup> GetSeminarGroupBySeminarGroupIdAsync(long seminarGroupId)
        {
            return _iGradeDao.GetSeminarGroupBySeminarGroupIdAsync(seminarGroupId);
        }

        public async Task<IList<SeminarGroup>> ListSeminarGradeByCourseIdAsync(long userId, long courseId)
        {
            var seminarList =  await _iSeminarService.ListSeminarByCourseIdAsync(courseId);

                //调用SeminarGroupService 中 SeminarGroup GetSeminarGroupByIdAsync(long seminarId, long userId)


            return await Task.WhenAll(seminarList.Select(async t => await _iSeminarGroupService.GetSeminarGroupByIdAsync(t.Id, userId)));
        }

        public async Task InsertGroupGradeByUserIdAsync(long topicId, long userId, long groupId, int grade)
        {

                //调用TopicService中GetSeminarGroupTopicById(long topicId, long groupId)方法 
                var seminarGroupTopic = await _iTopicService.GetSeminarGroupTopicByIdAsync(topicId, groupId);
                //调用UserService中的GetUserByUserId(long userId)方法
                var userInfo = await _iUserService.GetUserByUserIdAsync(userId);
                //调用自己的dao
                await _iGradeDao.InsertGroupGradeByUserIdAsync(seminarGroupTopic, userInfo, grade);
        }

        public Task UpdateGroupByGroupIdAsync(long seminarGroupId, int grade)
        {

           return _iGradeDao.UpdateGroupByGroupIdAsync(seminarGroupId, grade);

        }

        public async Task CountPresentationGradeAsync(long seminarId)
        {
                //调用TopicService 的 IList<Topic> ListTopicBySeminarIdAsync(long seminarId)方法
                var topicList = await _iTopicService.ListTopicBySeminarIdAsync(seminarId);

                //调用自己的dao分别对每个topic计算
                await _iGradeDao.CountPresentationGradeAsync(seminarId, topicList);
        }

        public async Task CountGroupGradeBySerminarIdAsync(long seminarId)
        {
                ////调用SeminarGroupService中的ListSeminarGroupBySeminarId 方法
                var seminarGroupList = await _iSeminarGroupService.ListSeminarGroupBySeminarIdAsync(seminarId);

            await _iGradeDao.CountGroupGradeBySerminarIdAsync(seminarId, seminarGroupList);

        }
    }
}