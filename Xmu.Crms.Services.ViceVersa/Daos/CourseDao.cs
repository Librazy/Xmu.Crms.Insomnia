using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.EntityFrameworkCore;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;

namespace Xmu.Crms.Services.ViceVersa.Daos
{
    public class CourseDao : ICourseDao
    {
        private readonly CrmsContext _db;

        public CourseDao(CrmsContext db) => _db = db;

        public void DeleteCourseByCourseId(long courseId)
        {
            try
            {
                var course = _db.Course.Where(c => c.Id == courseId).SingleOrDefault();
                if (course == null)
                {
                    throw new CourseNotFoundException();
                }

                //将实体附加到对象管理器中
                _db.Course.Attach(course);
                //删除
                _db.Course.Remove(course);
                _db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public Course GetCourseByCourseId(BigInteger courseId)
        {
            try
            {
                var course = _db.Course.Include(c => c.Teacher).SingleOrDefault(c => c.Id == courseId);
                return course;
            }
            catch
            {
                throw;
            }
        }

        public long InsertCourseByUserId(Course course)
        {
            try
            {
                _db.Course.Add(course);
                _db.SaveChanges();
                return course.Id; //SaveChanges后Id变成了数据库里创建完course后自增的那个Id
            }
            catch
            {
                throw;
            }
        }


        public List<Course> ListCourseByCourseName(string courseName)
        {
            try
            {
                var courseList = _db.Course.Include(c => c.Teacher).Where(c => c.Name == courseName).ToList();
                return courseList;
            }
            catch
            {
                throw;
            }
        }


        public List<Course> ListCourseByUserId(long userId)
        {
            List<Course> courseList = null;
            try
            {
                courseList = _db.Course.Where(u => u.Teacher.Id == userId).ToList();
            }
            catch
            {
                throw;
            }

            return courseList;
        }

        public void UpdateCourseByCourseId(long courseId, Course course)
        {
            try
            {
                var cour = _db.Course.SingleOrDefault(c => c.Id == courseId);
                //如果找不到该课程
                if (cour == null)
                {
                    throw new CourseNotFoundException();
                }

                //更新该课程(更新界面上能够更改的内容)
                cour.Name = course.Name;
                cour.StartDate = course.StartDate;
                cour.EndDate = course.EndDate;
                cour.Description = course.Description;
                _db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        //添加班级返回id
        public long Save(ClassInfo t)
        {
            using (var scope = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.ClassInfo.Add(t);

                    _db.SaveChanges();

                    scope.Commit();
                    return t.Id;
                }
                catch
                {
                    scope.Rollback();
                    throw;
                }
            }
        }
    }
}