using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;

namespace Xmu.Crms.Services.ViceVersa
{
    internal class ClassDao : IClassDao
    {
        private readonly CrmsContext _db;

        public ClassDao(CrmsContext db) => _db = db;

        //删除班级和学生选课表
        public async Task DeleteAsync(long id)
        {
            using (var scope = _db.Database.BeginTransaction())
            {
                try
                {
                    var c = _db.ClassInfo.SingleOrDefault(u => u.Id == id);

                    if (c == null)
                    {
                        throw new ClassNotFoundException();
                    }

                    //根据class信息删除courseSelection表
                    await DeleteSelectionAsync(0, c.Id);

                    _db.ClassInfo.Attach(c);
                    _db.ClassInfo.Remove(c);
                    await _db.SaveChangesAsync();
                    scope.Commit();
                }
                catch (ClassNotFoundException e)
                {
                    scope.Rollback();
                    throw e;
                }
            }
        }

        public async Task<ClassInfo> GetAsync(long id)
        {
            var classinfo = await _db.ClassInfo.Include(u => u.Course).Include(u => u.Course.Teacher)
                .SingleOrDefaultAsync(u => u.Id == id);

            if (classinfo == null)
            {
                throw new ClassNotFoundException();
            }

            return classinfo;
        }

        //根据课程id列出所有班级
        public async Task<IList<ClassInfo>> QueryAllAsync(long id)
        {
            //找到这门课
            var course = await _db.Course.SingleOrDefaultAsync(u => u.Id == id);
            if (course == null)
            {
                throw new CourseNotFoundException();
            }

            return await _db.ClassInfo.Include(u => u.Course).Include(u => u.Course.Teacher)
                .Include(u => u.Course.Teacher.School).Where(u => u.Course.Id == id).ToListAsync();
        }

        //添加学生选课表返回id
        public long InsertSelection(CourseSelection t)
        {
            using (var scope = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.CourseSelection.Add(t);

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

        //查询学生选课表的记录
        public int GetSelection(long userId, long classId)
        {
            var courseSelection = _db.CourseSelection
                .SingleOrDefault(u => u.ClassInfo.Id == classId && u.Student.Id == userId);

            if (courseSelection != null)
            {
                return 1; //找到记录
            }

            return 0;
        }

        public async Task UpdateAsync(ClassInfo t)
        {
            using (var scope = _db.Database.BeginTransaction())
            {
                try
                {
                    var c = _db.ClassInfo.SingleOrDefault(u => u.Id == t.Id);

                    if (c == null)
                    {
                        throw new ClassNotFoundException();
                    }

                    c.Name = t.Name;
                    c.Course = t.Course;
                    c.Site = t.Site;
                    c.ClassTime = t.ClassTime;
                    c.ReportPercentage = t.ReportPercentage;
                    c.PresentationPercentage = t.PresentationPercentage;
                    c.FivePointPercentage = t.FivePointPercentage;
                    c.FourPointPercentage = t.FourPointPercentage;
                    c.ThreePointPercentage = t.ThreePointPercentage;

                    _db.Entry(c).State = EntityState.Modified;
                    await _db.SaveChangesAsync();

                    scope.Commit();
                }
                catch
                {
                    scope.Rollback();
                    throw;
                }
            }
        }

        //根据班级id/学生id删除学生选课表
        public async Task DeleteSelectionAsync(long userId, long classId)
        {
            if (userId != 0) //单个学生取消选课
            {
                using (var scope = _db.Database.BeginTransaction())
                {
                    try
                    {
                        var c = _db.CourseSelection.SingleOrDefault(u =>
                            u.Student.Id == userId && u.ClassInfo.Id == classId);

                        _db.CourseSelection.Attach(c);
                        _db.CourseSelection.Remove(c);
                        await _db.SaveChangesAsync();
                        scope.Commit();
                    }
                    catch
                    {
                        scope.Rollback();
                        throw;
                    }
                }
            }

            else //删除班级时 批量删除
            {
                var t1 = _db.CourseSelection.Where(t => t.ClassInfo.Id == classId).ToList();

                foreach (var t in t1)
                {
                    _db.CourseSelection.Remove(t);
                }

                await _db.SaveChangesAsync();
            }
        }

        // 根据学生ID获取班级列表
        public async Task<List<ClassInfo>> ListClassByUserIdAsync(long userId)
        {
            var selectionList = await _db.CourseSelection.Include(c => c.Student).Include(c => c.Student.School)
                .Include(c => c.ClassInfo).Include(c => c.ClassInfo.Course.Teacher.School)
                .Where(c => c.Student.Id == userId).ToListAsync();
            //找不到对应的选课信息
            if (selectionList == null)
            {
                throw new ClassNotFoundException();
            }

            //根据classId获得对应的class
            var classList = new List<ClassInfo>();

            foreach (var i in selectionList)
            {
                classList.Add(await GetAsync(i.ClassInfo.Id));
            }

            return classList;
        }

        // 老师获取该班级签到、分组状态.
        public Task<Location> GetLocation(long seminarId, long classId)
        {
            return _db.Location.Include(u => u.ClassInfo).Include(u => u.Seminar)
                .SingleOrDefaultAsync(u => u.Seminar.Id == seminarId && u.ClassInfo.Id == classId);
        }

        //添加Location表返回id
        public async Task<long> InsertLocationAsync(Location t)
        {
            using (var scope = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.Location.Add(t);

                    await _db.SaveChangesAsync();

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

        //结束签到时修改location
        public async Task<int> UpdateLocationAsync(long seminarId, long classId)
        {
            using (var scope = _db.Database.BeginTransaction())
            {
                try
                {
                    var location = await _db.Location.Include(u => u.Seminar).Include(u => u.ClassInfo)
                        .SingleOrDefaultAsync(u => u.ClassInfo.Id == classId && u.Seminar.Id == seminarId);
                    //没有记录
                    if (location == null)
                    {
                        throw new ClassNotFoundException();
                    }

                    location.Status = 0;
                    _db.Entry(location).State = EntityState.Modified;
                    await _db.SaveChangesAsync();

                    scope.Commit();
                    return 0;
                }
                catch
                {
                    scope.Rollback();
                    throw new ClassNotFoundException();
                }
            }
        }
    }
}