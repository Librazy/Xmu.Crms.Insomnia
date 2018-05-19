using System.Collections.Generic;
using System.Threading.Tasks;
using Xmu.Crms.Shared.Models;

namespace Xmu.Crms.Shared.Service
{
    /// <summary>
    ///     @author YeXiaona ZhouZhongjun CaoXingmei
    ///     @version 2.00
    /// </summary>
    public interface ICourseService : Orleans.IGrainWithGuidKey
    {
        /// <summary>
        ///     新建班级.
        ///     @author yexiaona
        /// </summary>
        /// <param name="courseId">课程id</param>
        /// <param name="classInfo">班级信息</param>
        /// <returns>classId 班级Id</returns>
        Task<long> InsertClassByIdAsync(long courseId, ClassInfo classInfo);


        /// <summary>
        ///     按userId获取与当前用户相关联的课程列表.
        ///     @author ZhouZhongjun
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns>null 课程列表</returns>
        /// <exception cref="T:System.ArgumentException">userId格式错误时抛出</exception>
        Task<IList<Course>> ListCourseByUserIdAsync(long userId);


        /// <summary>
        ///     按userId创建课程.
        ///     @author ZhouZhongjun
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="course">课程信息</param>
        /// <returns>courseId 新建课程的id</returns>
        /// <exception cref="T:System.ArgumentException">userId格式错误时抛出</exception>
        Task<long> InsertCourseByUserIdAsync(long userId, Course course);


        /// <summary>
        ///     按courseId获取课程 .
        ///     @author ZhouZhongjun
        /// </summary>
        /// <param name="courseId">课程Id</param>
        /// <returns>course</returns>
        /// <exception cref="T:System.ArgumentException">userId格式错误时抛出</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.CourseNotFoundException">未找到课程</exception>
        Task<Course> GetCourseByCourseIdAsync(long courseId);


        /// <summary>
        ///     传入courseId和course信息修改course信息.
        ///     @author ZhouZhongjun
        /// </summary>
        /// <param name="courseId">课程Id</param>
        /// <param name="course">课程信息</param>
        Task UpdateCourseByCourseIdAsync(long courseId, Course course);


        /// <summary>
        ///     按courseId删除课程.
        ///     @author ZhouZhongjun
        /// </summary>
        /// <param name="courseId">课程Id</param>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ISeminarService.DeleteSeminarByCourseIdAsync(System.Int64)" />
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IClassService.DeleteClassByCourseIdAsync(System.Int64)" />
        /// <exception cref="T:System.ArgumentException">courseId格式错误时抛出</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.CourseNotFoundException">未找到课程</exception>
        Task DeleteCourseByCourseIdAsync(long courseId);


        /// <summary>
        ///     根据课程名称获取课程列表.
        ///     @author YeXiaona
        /// </summary>
        /// <param name="courseName">课程名称</param>
        /// <returns>list 课程列表</returns>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ICourseService.GetCourseByCourseIdAsync(System.Int64)" />
        Task<IList<Course>> ListCourseByCourseNameAsync(string courseName);


        /// <summary>
        ///     按课程名称获取班级列表.
        ///     @author YeXiaona
        /// </summary>
        /// <param name="courseName">课程名称</param>
        /// <returns>list 班级列表</returns>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ICourseService.ListCourseByCourseNameAsync(System.String)" />
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IClassService.ListClassByCourseIdAsync(System.Int64)" />
        Task<IList<ClassInfo>> ListClassByCourseNameAsync(string courseName);


        /// <summary>
        ///     按教师名称获取班级列表.
        ///     @author YeXiaona
        /// </summary>
        /// <param name="teacherName">教师名称</param>
        /// <returns>list 班级列表</returns>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IUserService.ListUserIdByUserNameAsync(System.String)" />
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ICourseService.ListClassByUserIdAsync(System.Int64)" />
        Task<IList<ClassInfo>> ListClassByTeacherNameAsync(string teacherName);


        /// <summary>
        ///     根据学生ID获取班级列表.
        ///     @author YeXiaona
        /// </summary>
        /// <param name="userId">学生ID</param>
        /// <returns>list 班级列表</returns>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ICourseService.ListCourseByUserIdAsync(System.Int64)" />
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IClassService.ListClassByCourseIdAsync(System.Int64)" />
        /// <exception cref="T:System.ArgumentException">userId格式错误时抛出</exception>
        Task<IList<ClassInfo>> ListClassByNameAsync(string courseName, string teacherName);

        /// <summary>
        ///     根据教师名称列出课程名称.
        ///     @author yexiaona
        /// </summary>
        /// <param name="teacherName">教师名称</param>
        /// <returns>list 课程列表</returns>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IUserService.ListUserByUserNameAsync(System.String)" />
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ICourseService.ListCourseByUserIdAsync(System.Int64)" />
        Task<IList<Course>> 
            ListCourseByTeacherNameAsync(string teacherName);
    }
}