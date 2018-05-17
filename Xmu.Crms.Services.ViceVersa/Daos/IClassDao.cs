using System.Collections.Generic;
using Xmu.Crms.Shared.Models;

namespace Xmu.Crms.Services.ViceVersa
{
    internal interface IClassDao
    {
        long InsertSelection(CourseSelection t);
        long InsertLocation(Location t);
        void Delete(long id);
        void DeleteSelection(long userId, long classId);
        int Update(ClassInfo t);
        int UpdateLocation(long seminarId, long classId);
        List<ClassInfo> QueryAll(long id);
        ClassInfo Get(long id);
        int GetSelection(long userId, long classId);

        Location GetLocation(long seminarId, long classId);

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
        List<ClassInfo> ListClassByUserId(long userId);
    }
}