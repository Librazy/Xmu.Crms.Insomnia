using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Concurrency;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;

namespace Xmu.Crms.Services.ViceVersa
{
    [StatelessWorker]
    internal class ClassService : Orleans.Grain, IClassService
    {
        private readonly IClassDao _classDao;
        private readonly IFixGroupService _fixGroupService;
        private readonly ISeminarService _seminarService;
        private readonly IUserService _userService;

        public ClassService(ISeminarService seminarService, IUserService userService, IFixGroupService fixGroupService,
            IClassDao classDao)
        {
            _seminarService = seminarService;
            _userService = userService;
            _fixGroupService = fixGroupService;
            _classDao = classDao;
        }

        /// 按班级id删除班级.(包括学生选课表)
        public Task DeleteClassByClassIdAsync(long classId) => _classDao.DeleteAsync(classId);


        /// 按courseId删除Class.
        public async Task DeleteClassByCourseIdAsync(long courseId)
        {
            //根据课程id获得所有班级信息
            var deleteClasses = await _classDao.QueryAllAsync(courseId);
            foreach (var c in deleteClasses)
            {
                await _fixGroupService.DeleteFixGroupByClassIdAsync(c.Id);
                // 根据class信息删除courseSelection表的记录 并删除班级
                await _classDao.DeleteAsync(c.Id);
            }
        }


        /// 按classId删除CourseSelection表的一条记录.  ？？和取消选课的区别
        public Task DeleteClassSelectionByClassIdAsync(long classId) => _classDao.DeleteSelectionAsync(0, classId);


        /// 学生按班级id取消选择班级.
        public Task DeleteCourseSelectionByIdAsync(long userId, long classId)
        {
            //_userService.GetUserByUserIdAsync(userId);
            GetClassByClassIdAsync(classId);
            return _classDao.DeleteSelectionAsync(userId, classId);
        }


        /// 老师获取该班级签到、分组状态.
        public Task<Location> GetCallStatusByIdAsync(long seminarId, long classId)
        {
            _seminarService.GetSeminarBySeminarIdAsync(seminarId);
            //_classDao.GetAsync(classId);
            return _classDao.GetLocation(seminarId, classId);
        }


        /// 按班级id获取班级详情.
        public Task<ClassInfo> GetClassByClassIdAsync(long classId) => _classDao.GetAsync(classId);


        /// 学生按班级id选择班级.成功返回选课记录id 失败返回0
        public async Task<long> InsertCourseSelectionByIdAsync(long userId, long classId)
        {
            //_userService.GetUserByUserIdAsync(userId);
            var classinfo = await GetClassByClassIdAsync(classId);

            //找到该班级所属课程下的所有班级
            var classList = await ListClassByCourseIdAsync(classinfo.Course.Id);
            foreach (var c in classList)
            {
                if (_classDao.GetSelection(userId, c.Id) != 0) //学生已选同课程下其他班级
                {
                    return 0;
                }
            }

            var coursesele = new CourseSelection();


            var student = await _userService.GetUserByUserIdAsync(userId);
            coursesele.Student = student;
            coursesele.ClassInfo = classinfo;
            return _classDao.InsertSelection(coursesele);
        }


        /// 根据课程ID获得班级列表.
        public Task<IList<ClassInfo>> ListClassByCourseIdAsync(long courseId) => _classDao.QueryAllAsync(courseId);

        //修改班级
        public async Task UpdateClassByClassIdAsync(long classId, ClassInfo newclass)
        {
            await _classDao.UpdateAsync(newclass); //return 0成功更新
        }

        //根据学生ID获取班级列表.
        public Task<List<ClassInfo>> ListClassByUserIdAsync(long userId) => _classDao.ListClassByUserIdAsync(userId);

        //老师发起签到.
        public async Task<long> CallInRollByIdAsync(Location location)
        {
            location.Seminar = await _seminarService.GetSeminarBySeminarIdAsync(location.Seminar.Id);
            location.ClassInfo = await GetClassByClassIdAsync(location.ClassInfo.Id);
            location.Status = 1;
            return await _classDao.InsertLocationAsync(location);
        }

        //老师结束签到.
        public async Task EndCallRollByIdAsync(long seminarId, long classId)
        {
            //_seminarService.GetSeminarBySeminarIdAsync(seminarId);
            await GetClassByClassIdAsync(classId);
            await _classDao.UpdateLocationAsync(seminarId, classId);
        }


        /// 按classId删除ScoreRule.
        public async Task DeleteScoreRuleByIdAsync(long classId)
        {
            var newclass = new ClassInfo
            {
                Id = classId,
                ReportPercentage = 0,
                PresentationPercentage = 0,
                FivePointPercentage = 0,
                FourPointPercentage = 0,
                ThreePointPercentage = 0
            };
            await _classDao.UpdateAsync(newclass);
        }


        /// 查询评分规则.
        public async Task<ClassInfo> GetScoreRuleAsync(long classId) => await _classDao.GetAsync(classId);


        /// 新增评分规则.  返回班级id
        public async Task<long> InsertScoreRuleAsync(long classId, ClassInfo proportions)
        {
            if (proportions.ReportPercentage < 0 || proportions.ReportPercentage > 100 ||
                proportions.PresentationPercentage < 0 || proportions.PresentationPercentage > 100 ||
                proportions.ReportPercentage + proportions.PresentationPercentage != 100 ||
                proportions.FivePointPercentage < 0 || proportions.FivePointPercentage > 10 ||
                proportions.FourPointPercentage < 0 || proportions.FourPointPercentage > 10 ||
                proportions.ThreePointPercentage < 0 || proportions.ThreePointPercentage > 10 ||
                proportions.FivePointPercentage + proportions.FourPointPercentage +
                proportions.ThreePointPercentage != 10)
            {
                throw new InvalidOperationException();
            }

            await _classDao.UpdateAsync(proportions); //新建班级时已经建了一个空的

            return classId;
        }


        /// 修改评分规则.
        public async Task UpdateScoreRuleAsync(long classId, ClassInfo proportions)
        {
            if (proportions.ReportPercentage < 0 || proportions.ReportPercentage > 100 ||
                proportions.PresentationPercentage < 0 || proportions.PresentationPercentage > 100 ||
                proportions.ReportPercentage + proportions.PresentationPercentage != 100 ||
                proportions.FivePointPercentage < 0 || proportions.FivePointPercentage > 10 ||
                proportions.FourPointPercentage < 0 || proportions.FourPointPercentage > 10 ||
                proportions.ThreePointPercentage < 0 || proportions.ThreePointPercentage > 10 ||
                proportions.FivePointPercentage + proportions.FourPointPercentage +
                proportions.ThreePointPercentage != 10)
            {
                throw new InvalidOperationException();
            }

            await _classDao.UpdateAsync(proportions); //新建班级时已经建了一个空的
        }
    }
}