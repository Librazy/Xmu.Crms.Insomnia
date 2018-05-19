using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;

namespace Xmu.Crms.Services.HighGrade
{
    public class SeminarService : Orleans.Grain, ISeminarService
    {
        private readonly CrmsContext _db;
        public SeminarService(CrmsContext db)
        {
            _db = db;
        }

        /// <summary>
        ///     按courseId获取Seminar.
        ///     @author zhouzhongjun
        /// </summary>
        /// <param name="courseId">课程Id</param>
        /// <returns>List 讨论课列表</returns>
        /// <exception cref="ArgumentException">格式错误、教师设置embedGrade为true时抛出</exception>
        /// <exception cref="CourseNotFoundException">未找到该课程时抛出</exception>
        public async Task<IList<Seminar>> ListSeminarByCourseIdAsync(long courseId)
        {
            var seminars = await _db.Seminar.Where(s => s.Course.Id == courseId).ToListAsync();
            if (seminars == null)
            {
                throw new SeminarNotFoundException();
            }

            return seminars;
        }


        /// <summary>
        ///     按courseId删除Seminar.
        ///     @author zhouzhongjun
        /// </summary>
        /// 先根据CourseId获得所有的seminar的信息，然后根据seminar信息删除相关topic的记录，然后再根据SeminarId删除SeminarGroup表记录,最后再将seminar的信息删除
        /// <param name="courseId">课程Id</param>
        /// <returns>true删除成功 false删除失败</returns>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ISeminarService.ListSeminarByCourseIdAsync(System.Int64)" />
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ITopicService.DeleteTopicBySeminarIdAsync(System.Int64)" />
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ISeminarGroupService.DeleteSeminarGroupBySeminarIdAsync(System.Int64)" />
        /// <exception cref="ArgumentException">格式错误时抛出</exception>
        /// <exception cref="CourseNotFoundException">该课程不存在时抛出</exception>
        public async Task DeleteSeminarByCourseIdAsync(long courseId)
        {
            if (courseId < 0)
            {
                throw new ArgumentException();
            }

            var seminars = await _db.Seminar.Where(_seminar => _seminar.Course.Id == courseId).ToListAsync();

            if (seminars == null)
            {
                throw new SeminarNotFoundException();
            }

            for (var i = 0; i <= seminars.Count; i++)
            {
                _db.Seminar.Remove(seminars[i]);
            }

            await _db.SaveChangesAsync();
        }

        /// <summary>
        ///     用户通过讨论课id获得讨论课的信息.
        ///     @author CaoXingmei
        /// </summary>
        /// 用户通过讨论课id获得讨论课的信息（包括讨论课名称、讨论课描述、分组方式、开始时间、结束时间）
        /// <param name="seminarId">讨论课的id</param>
        /// <returns>相应的讨论课信息</returns>
        /// <exception cref="ArgumentException">格式错误时抛出</exception>
        /// <exception cref="CourseNotFoundException">该课程不存在时抛出</exception>
        public async Task<Seminar> GetSeminarBySeminarIdAsync(long seminarId)
        {
            if (seminarId < 0)
            {
                throw new ArgumentException();
            }

            var seminar = await _db.Seminar.SingleOrDefaultAsync(s => s.Id == seminarId);
            if (seminar == null) //I add it myself
            {
                throw new SeminarNotFoundException();
            }

            return seminar;
        }


        /// <summary>
        ///     按讨论课id修改讨论课.
        ///     @author CaoXingmei
        /// </summary>
        /// 用户（老师）通过seminarId修改讨论课的相关信息
        /// <param name="seminarId">讨论课的id</param>
        /// <param name="updated">讨论课信息</param>
        /// <returns>true(修改成功), false(修改失败)</returns>
        /// <exception cref="ArgumentException">格式错误时抛出</exception>
        /// <exception cref="SeminarNotFoundException">该讨论课不存在时抛出</exception>
        public async Task UpdateSeminarBySeminarIdAsync(long seminarId, Seminar updated)
        {
            if (seminarId < 0)
            {
                throw new ArgumentException();
            }

            //这个是引用吗
            var seminar = await _db.Seminar.FindAsync(seminarId) ?? throw new SeminarNotFoundException();

            seminar.Description = updated.Description;
            seminar.StartTime = updated.StartTime;
            seminar.EndTime = updated.EndTime;
            seminar.Name = updated.Name;
            seminar.IsFixed = updated.IsFixed;
            await _db.SaveChangesAsync();
        }


        /// <summary>
        ///     按讨论课id删除讨论课.
        ///     @author CaoXingmei
        /// </summary>
        /// 用户（老师）通过seminarId删除讨论课(包括删除讨论课包含的topic信息和小组信息).
        /// <param name="seminarId">讨论课的id</param>
        /// <returns>true(删除成功), false(删除失败)</returns>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ISeminarGroupService.DeleteSeminarGroupBySeminarIdAsync(System.Int64)" />
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ITopicService.DeleteTopicBySeminarIdAsync(System.Int64)" />
        /// <exception cref="ArgumentException">格式错误时抛出</exception>
        /// <exception cref="SeminarNotFoundException">该讨论课不存在时抛出</exception>
        public async Task DeleteSeminarBySeminarIdAsync(long seminarId)
        {
            if (seminarId < 0)
            {
                throw new ArgumentException();
            }

            var seminars = _db.Seminar.Where(_seminar => _seminar.Id == seminarId).ToList();

            if (seminars == null)
            {
                throw new SeminarNotFoundException();
            }

            for (var i = 0; i <= seminars.Count; i++)
            {
                _db.Seminar.Remove(seminars[i]);
            }

            await _db.SaveChangesAsync();
        }


        /// <summary>
        ///     新增讨论课.
        ///     @author YeHongjie
        /// </summary>
        /// 用户（老师）在指定的课程下创建讨论课
        /// <param name="courseId">课程的id</param>
        /// <param name="seminar">讨论课信息</param>
        /// <returns>seminarId 若创建成功返回创建的讨论课id，失败则返回-1</returns>
        /// <exception cref="ArgumentException">格式错误时抛出</exception>
        /// <exception cref="SeminarNotFoundException">该讨论课不存在时抛出</exception>
        public async Task<long> InsertSeminarByCourseIdAsync(long courseId, Seminar seminar)
        {
            if (seminar == null)
            {
                throw new SeminarNotFoundException();
            }

            if (courseId < 0)
            {
                throw new ArgumentException();
            }

            var course = _db.Course.SingleOrDefault(_course => _course.Id == courseId);
            seminar.Course = course;

            _db.Seminar.Add(seminar);
            await _db.SaveChangesAsync();
            return seminar.Id;
        }
    }
}