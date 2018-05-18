using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Xmu.Crms.Shared.Models;

namespace Xmu.Crms.Services.ViceVersa.Daos
{
    public interface ICourseDao
    {
        /**
         * 按userId获取与当前用户相关联的课程列表.
         * <p>老师与他相关联的课程列表<br> 
         * @author ZhouZhongjun
         * @param userId 用户Id
         * @return null 课程列表
         * @exception ArgumentException userId格式错误时抛出
         * @exception CourseNotFoundException 未找到课程
         */
        Task<List<Course>> ListCourseByUserIdAsync(long userId);


        /// <summary>
        ///     按userId创建课程.
        ///     @author ZhouZhongjun
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="course">课程信息</param>
        /// <returns>courseId 新建课程的id</returns>
        /// <exception cref="T:System.ArgumentException">userId格式错误时抛出</exception>
        Task<long> InsertCourseByUserIdAsync(Course course);


        /**
         * 按courseId获取课程 .
         * @author ZhouZhongjun
         * @param courseId 课程Id
         * @return course
         * @exception InfoIllegalException courseId格式错误时抛出
         * @exception CourseNotFoundException 未找到课程
         */
        Task<Course> GetCourseByCourseIdAsync(BigInteger courseId);


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


        /**
         * 根据课程名称获取课程列表.
         * <p>根据课程名称获取课程列表<br>
         * @author YeXiaona
         * @param courseName 课程名称
         * @return list 课程列表
         * @see CourseService #getCourseByCourseId(BigInteger courseId)
         * @exception InfoIllegalException courseId格式错误时抛出
         * @exception CourseNotFoundException 未找到课程
         */
        /// <summary>
        ///     根据课程名称获取课程列表.
        ///     @author YeXiaona
        /// </summary>
        /// <param name="courseName">课程名称</param>
        /// <returns>list 课程列表</returns>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ICourseService.GetCourseByCourseIdAsync(System.Int64)" />
        Task<List<Course>> ListCourseByCourseNameAsync(string courseName);


        /**
         * 按教师名称获取班级列表.
         * <p>根据教师名称获取课程ID，通过课程ID获取班级列表<br>
         * @author YeXiaona
         * @param teacherName 教师名称
         * @return list 班级列表
         * @see UserService #listUserIdByUserName(String userName)
         * @see CourseService #listClassByUserId(BigInteger userId)
         * @exception UserNotFoundException 未找到用户
         * @exception CourseNotFoundException 未找到课程
         * @exception ClassNotFoundException 未找到班级
         */


        /**
         * 根据教师ID获取班级列表.  
         * @author YeXiaona
         * @param userId 教师ID
         * @return list 班级列表
         * @see CourseService #listCourseByUserId(BigInteger userId)
         * @see ClassService #listClassByCourseId(BigInteger courseId)
         * @exception InfoIllegalException userId格式错误时抛出
         * @exception InfoIllegalException courseId格式错误时抛出
         * @exception CourseNotFoundException 未找到课程
         * @exception ClassNotFoundException 未找到班级
         */
        //List<ClassInfo> ListClassByUserIdAsync(long userId);

        //新增班级
        long Save(ClassInfo t);
    }
}