using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;

namespace Xmu.Crms.Services.ViceVersa.Daos
{
    public class CourseDao : ICourseDao
    {
        private readonly CrmsContext _db;

        public CourseDao(CrmsContext db) => _db = db;

        public async Task DeleteCourseByCourseIdAsync(long courseId)
        {
            var course = await _db.Course.SingleOrDefaultAsync(c => c.Id == courseId);
            if (course == null)
            {
                throw new CourseNotFoundException();
            }

            //将实体附加到对象管理器中
            _db.Course.Attach(course);
            //删除
            _db.Course.Remove(course);
            await _db.SaveChangesAsync();
        }

        public async Task<Course> GetCourseByCourseIdAsync(BigInteger courseId)
        {
            return await _db.Course.Include(c => c.Teacher).SingleOrDefaultAsync(c => c.Id == courseId);
        }

        public async Task<long> InsertCourseByUserIdAsync(Course course)
        {
            _db.Course.Add(course);
            await _db.SaveChangesAsync();
            return course.Id; //SaveChanges后Id变成了数据库里创建完course后自增的那个Id
        }


        public Task<List<Course>> ListCourseByCourseNameAsync(string courseName)
        {
            return _db.Course.Include(c => c.Teacher).Where(c => c.Name == courseName).ToListAsync();
        }


        public Task<List<Course>> ListCourseByUserIdAsync(long userId)
        {
            return _db.Course.Where(u => u.Teacher.Id == userId).ToListAsync();
        }

        public async Task UpdateCourseByCourseIdAsync(long courseId, Course course)
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
            await _db.SaveChangesAsync();
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