using System.Collections.Generic;
using System.Threading.Tasks;
using Xmu.Crms.Shared.Models;

namespace Xmu.Crms.Services.ViceVersa
{
    internal interface IClassDao
    {
        long InsertSelection(CourseSelection t);
        Task<long> InsertLocationAsync(Location t);
        Task DeleteAsync(long id);
        Task DeleteSelectionAsync(long userId, long classId);
        Task UpdateAsync(ClassInfo t);
        Task<int> UpdateLocationAsync(long seminarId, long classId);
        Task<IList<ClassInfo>> QueryAllAsync(long id);
        Task<ClassInfo> GetAsync(long id);
        int GetSelection(long userId, long classId);

        Task<Location> GetLocation(long seminarId, long classId);

        /**
         * 根据学生ID获取班级列表.  
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
        Task<List<ClassInfo>> ListClassByUserIdAsync(long userId);
    }
}