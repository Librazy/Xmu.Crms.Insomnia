using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Concurrency;
using Xmu.Crms.Services.ViceVersa.Daos;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using Type = Xmu.Crms.Shared.Models.Type;

namespace Xmu.Crms.Services.ViceVersa.Services
{
    [StatelessWorker]
    internal class CourseService : Orleans.Grain, ICourseService
    {
        private readonly IClassService _iClassService;
        private readonly ICourseDao _iCourseDao;
        private readonly ISeminarService _iSeminarService;
        private readonly IUserService _iUserService;

        public CourseService(ICourseDao iCourseDao, IClassService iClassService, ISeminarService iSeminarService,
            IUserService iUserService)
        {
            _iCourseDao = iCourseDao;
            _iSeminarService = iSeminarService;
            _iClassService = iClassService;
            _iUserService = iUserService;
        }

        public async Task DeleteCourseByCourseIdAsync(long courseId)
        {
            if (courseId < 0)
            {
                throw new ArgumentException();
            }

            //删除course下的class
            await _iClassService.DeleteClassByCourseIdAsync(courseId);
            //删除course下的seminar
            await _iSeminarService.DeleteSeminarByCourseIdAsync(courseId);
            //删除course
            await _iCourseDao.DeleteCourseByCourseIdAsync(courseId);
        }

        public async Task<Course> GetCourseByCourseIdAsync(long courseId)
        {
            if (courseId < 0)
            {
                throw new ArgumentException();
            }

            var course = await _iCourseDao.GetCourseByCourseIdAsync(courseId);
            //没查到该门课
            if (course == null)
            {
                throw new CourseNotFoundException();
            }

            return course;
        }

        public async Task<long> InsertCourseByUserIdAsync(long userId, Course course)
        {
            if (userId < 0)
            {
                throw new ArgumentException();
            }

            //根据userId找出teacher
            var teacher = await _iUserService.GetUserByUserIdAsync(userId); //会抛出ArgumentException和UserNotFoundException
            course.Teacher = teacher;
            var courseId = await _iCourseDao.InsertCourseByUserIdAsync(course);
            return courseId;
        }

        public async Task<IList<ClassInfo>> ListClassByCourseNameAsync(string courseName)
        {
            //根据课程名获得对应的课程列表
            var courseList = await ListCourseByCourseNameAsync(courseName);
            //根据课程id获得该课程下的班级
            var classList = new List<ClassInfo>();
            foreach (var i in courseList)
            {
                classList.AddRange(await _iClassService.ListClassByCourseIdAsync(i.Id));
            }

            return classList;
        }

        public async Task<IList<ClassInfo>> ListClassByTeacherNameAsync(string teacherName)
        {
            var courseList = await ListCourseByTeacherNameAsync(teacherName);
            var classList = new List<ClassInfo>();
            foreach (var i in courseList)
            {
                classList.AddRange(await _iClassService.ListClassByCourseIdAsync(i.Id));
            }

            return classList;
        }

        public async Task<IList<Course>> ListCourseByCourseNameAsync(string courseName)
        {
            IList<Course> courseList = await _iCourseDao.ListCourseByCourseNameAsync(courseName);
            if (courseList == null || courseList.Count == 0)
            {
                throw new CourseNotFoundException();
            }

            return courseList;
        }

        public async Task<IList<Course>> ListCourseByUserIdAsync(long userId)
        {
            if (userId < 0)
            {
                throw new ArgumentException();
            }

            return await _iCourseDao.ListCourseByUserIdAsync(userId);
        }

        public async Task<IList<ClassInfo>> ListClassByNameAsync(string courseName, string teacherName)
        {
            var classList = new List<ClassInfo>();
            if (teacherName == null) //根据课程名称查
            {
                var courseClassList = await ListClassByCourseNameAsync(courseName);
                classList.AddRange(courseClassList);
            }
            else if (courseName == null) //根据教师姓名查
            {
                var teacherClassList = await ListClassByTeacherNameAsync(teacherName);
                classList.AddRange(teacherClassList);
            }
            else
            {
                var courseClassList = await ListClassByCourseNameAsync(courseName);
                var teacherClassList = await ListClassByTeacherNameAsync(teacherName);
                foreach (var cc in courseClassList)
                foreach (var ct in teacherClassList)
                {
                    if (cc.Id == ct.Id)
                    {
                        classList.Add(cc);
                        break;
                    }
                }
            }

            ////该学生已选班级列表
            //List<ClassInfo> studentClass = _classDao.ListClassByUserIdAsync(userId);
            //foreach (ClassInfo c in classList)
            //    foreach (ClassInfo cs in studentClass)
            //        if (c.Id == cs.Id) classList.Remove(c);//学生已选的就不列出

            return classList;
        }

        public Task UpdateCourseByCourseIdAsync(long courseId, Course course)
        {
            if (courseId < 0)
            {
                throw new ArgumentException();
            }

            return _iCourseDao.UpdateCourseByCourseIdAsync(courseId, course);
        }


        /// 新建班级.
        public async Task<long> InsertClassByIdAsync(long courseId, ClassInfo classInfo)
        {
            var course = await GetCourseByCourseIdAsync(courseId);
            //检查数据是否合法
            if (classInfo.ReportPercentage < 0 || classInfo.ReportPercentage > 100 ||
                classInfo.PresentationPercentage < 0 || classInfo.PresentationPercentage > 100 ||
                classInfo.ReportPercentage + classInfo.PresentationPercentage != 100 ||
                classInfo.FivePointPercentage < 0 || classInfo.FivePointPercentage > 100 ||
                classInfo.FourPointPercentage < 0 || classInfo.FourPointPercentage > 100 ||
                classInfo.ThreePointPercentage < 0 || classInfo.ThreePointPercentage > 100 ||
                classInfo.FivePointPercentage + classInfo.FourPointPercentage + classInfo.ThreePointPercentage !=
                100)
            {
                throw new InvalidOperationException();
            }

            classInfo.Course = course;
            return _iCourseDao.Save(classInfo); //返回classid
        }

        //根据教师名称列出课程名称
        public async Task<IList<Course>> ListCourseByTeacherNameAsync(string teacherName)
        {
            var users = await _iUserService.ListUserByUserNameAsync(teacherName);
            IList<Course> list = new List<Course>();
            foreach (var u in users)
            {
                if (u.Type == Type.Teacher)
                {
                    var temp = await ListCourseByUserIdAsync(u.Id);
                    foreach (var c in temp)
                    {
                        list.Add(c);
                    }
                }
            }

            return list;
        }
    }
}