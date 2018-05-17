using System;
using System.Collections.Generic;
using Xmu.Crms.Services.ViceVersa.Daos;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using Type = Xmu.Crms.Shared.Models.Type;

namespace Xmu.Crms.Services.ViceVersa.Services
{
    internal class CourseService : ICourseService
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

        public void DeleteCourseByCourseId(long courseId)
        {
            try
            {
                if (courseId < 0)
                {
                    throw new ArgumentException();
                }

                //删除course下的class
                _iClassService.DeleteClassByCourseId(courseId);
                //删除course下的seminar
                _iSeminarService.DeleteSeminarByCourseId(courseId);
                //删除course
                _iCourseDao.DeleteCourseByCourseId(courseId);
            }
            catch
            {
                throw;
            }
        }

        public Course GetCourseByCourseId(long courseId)
        {
            try
            {
                if (courseId < 0)
                {
                    throw new ArgumentException();
                }

                var course = _iCourseDao.GetCourseByCourseId(courseId);
                //没查到该门课
                if (course == null)
                {
                    throw new CourseNotFoundException();
                }

                return course;
            }
            catch
            {
                throw;
            }
        }

        public long InsertCourseByUserId(long userId, Course course)
        {
            try
            {
                if (userId < 0)
                {
                    throw new ArgumentException();
                }

                //根据userId找出teacher
                var teacher = _iUserService.GetUserByUserId(userId); //会抛出ArgumentException和UserNotFoundException
                course.Teacher = teacher;
                var courseId = _iCourseDao.InsertCourseByUserId(course);
                return courseId;
            }
            catch
            {
                throw;
            }
        }

        public IList<ClassInfo> ListClassByCourseName(string courseName)
        {
            try
            {
                //根据课程名获得对应的课程列表
                var courseList = ListCourseByCourseName(courseName);
                //根据课程id获得该课程下的班级
                var classList = new List<ClassInfo>();
                foreach (var i in courseList)
                {
                    classList.AddRange(_iClassService.ListClassByCourseId(i.Id));
                }

                return classList;
            }
            catch
            {
                throw;
            }
        }

        public IList<ClassInfo> ListClassByTeacherName(string teacherName)
        {
            try
            {
                var courseList = ListCourseByTeacherName(teacherName);
                var classList = new List<ClassInfo>();
                foreach (var i in courseList)
                {
                    classList.AddRange(_iClassService.ListClassByCourseId(i.Id));
                }

                return classList;
            }
            catch
            {
                throw;
            }
        }

        public IList<Course> ListCourseByCourseName(string courseName)
        {
            try
            {
                IList<Course> courseList = _iCourseDao.ListCourseByCourseName(courseName);
                if (courseList == null || courseList.Count == 0)
                {
                    throw new CourseNotFoundException();
                }

                return courseList;
            }
            catch
            {
                throw;
            }
        }

        public IList<Course> ListCourseByUserId(long userId)
        {
            if (userId < 0)
            {
                throw new ArgumentException();
            }

            var courseList = _iCourseDao.ListCourseByUserId(userId);
            //查不到课程
            return courseList;
        }

        public IList<ClassInfo> ListClassByName(string courseName, string teacherName)
        {
            try
            {
                var classList = new List<ClassInfo>();
                if (teacherName == null) //根据课程名称查
                {
                    var courseClassList = ListClassByCourseName(courseName);
                    classList.AddRange(courseClassList);
                }
                else if (courseName == null) //根据教师姓名查
                {
                    var teacherClassList = ListClassByTeacherName(teacherName);
                    classList.AddRange(teacherClassList);
                }
                else if (courseName != null && teacherName != null) //联合查找
                {
                    var courseClassList = ListClassByCourseName(courseName);
                    var teacherClassList = ListClassByTeacherName(teacherName);
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
                //List<ClassInfo> studentClass = _classDao.ListClassByUserId(userId);
                //foreach (ClassInfo c in classList)
                //    foreach (ClassInfo cs in studentClass)
                //        if (c.Id == cs.Id) classList.Remove(c);//学生已选的就不列出

                return classList;
            }
            catch (CourseNotFoundException ec)
            {
                throw ec;
            }
            catch (UserNotFoundException eu)
            {
                throw eu;
            }
        }

        public void UpdateCourseByCourseId(long courseId, Course course)
        {
            try
            {
                if (courseId < 0)
                {
                    throw new ArgumentException();
                }

                _iCourseDao.UpdateCourseByCourseId(courseId, course);
            }
            catch
            {
                throw;
            }
        }


        /// 新建班级.
        public long InsertClassById(long courseId, ClassInfo classInfo)
        {
            try
            {
                var course = GetCourseByCourseId(courseId);
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
            catch (CourseNotFoundException ec)
            {
                throw ec;
            }
        }

        //根据教师名称列出课程名称
        public IList<Course> ListCourseByTeacherName(string teacherName)
        {
            try
            {
                var users = _iUserService.ListUserByUserName(teacherName);
                IList<Course> list = new List<Course>();
                foreach (var u in users)
                {
                    if (u.Type == Type.Teacher)
                    {
                        var temp = ListCourseByUserId(u.Id);
                        foreach (var c in temp)
                        {
                            list.Add(c);
                        }
                    }
                }

                return list;
            }
            catch
            {
                throw;
            }
        }

        //移到classService
        public IList<ClassInfo> ListClassByUserId(long userId)
        {
            try
            {
                if (userId < 0)
                {
                    throw new ArgumentException();
                }

                var classList = _iClassService.ListClassByUserId(userId);
                //没有查到
                if (classList == null || classList.Count == 0)
                {
                    throw new ClassNotFoundException();
                }

                return classList;
            }
            catch
            {
                throw;
            }
        }
    }
}