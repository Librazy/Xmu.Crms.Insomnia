using System.Collections.Generic;
using System.Threading.Tasks;
using Xmu.Crms.Shared.Models;

namespace Xmu.Crms.Services.ViceVersa.Daos
{
    public interface IGradeDao
    {
        /// <summary>
        ///     按topicId删除学生打分表.
        ///     @author zhouzhongjun
        /// </summary>
        /// <param name="topicId">话题Id</param>
        Task DeleteStudentScoreGroupByTopicIdAsync(long topicId);

        /// <summary>
        ///     获取某学生一堂讨论课信息
        ///     @author qinlingyun
        ///     获取某学生一堂讨论课的详细信息（包括成绩）
        /// </summary>
        /// <param name="userId">学生Id</param>
        /// <param name="seminarGroupId">讨论课小组Id</param>
        /// <returns>seminarGroup 讨论课小组信息（包括成绩）</returns>
        /// <exception cref="T:System.ArgumentException">id格式错误</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.GroupNotFoundException">未找到小组</exception>
        Task<SeminarGroup> GetSeminarGroupBySeminarGroupIdAsync(long seminarGroupId);

        /// <summary>
        ///     按课程id获取学生该课程所有讨论课
        ///     @author qinlingyun
        ///     通过课程id获取该课程下学生所有讨论课详细信息（包括成绩）
        /// </summary>
        /// <param name="userId">学生id</param>
        /// <param name="courseId">课程id</param>
        /// <returns>list 该课程下所有讨论课列表</returns>
        /// <exception cref="T:System.ArgumentException">id格式错误</exception>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ISeminarService.ListSeminarByCourseIdAsync(System.Int64)" />
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IGradeService.ListSeminarGradeByUserId(System.Int64)" />
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ISeminarGroupService.ListSeminarGroupBySeminarIdAsync(System.Int64)" />
        List<SeminarGroup> ListSeminarGradeByCourseId(long userId, long courseId);

        /// <summary>
        ///     提交对其他小组的打分.
        ///     @author Huhui
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="topicId">话题Id</param>
        /// <param name="seminarId">讨论课Id</param>
        /// <param name="groupId">小组Id</param>
        /// <param name="grade">分数</param>
        Task InsertGroupGradeByUserIdAsync(SeminarGroupTopic seminarGroupTopic, UserInfo userInfo, int grade);

        /// <summary>
        ///     按ID设置小组报告分.
        ///     @author Huhui
        /// </summary>
        /// <param name="seminarGroupId">讨论课组id</param>
        /// <param name="grade">分数</param>
        Task UpdateGroupByGroupIdAsync(long seminarGroupId, int grade);

        /// <summary>
        ///     获取某学生的讨论课成绩列表.
        ///     @author qinlingyun
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ISeminarGroupService.ListSeminarGroupBySeminarIdAsync(System.Int64)" />
        List<SeminarGroup> ListSeminarGradeByStudentId(long userId);

        /// <summary>
        ///     定时器方法:讨论课结束后计算展示得分.
        ///     @author qinlingyun
        ///     条件: 讨论课已结束  *GradeService
        /// </summary>
        /// <param name="seminarId">讨论课id</param>
        /// <param name="seminarGroupId">讨论课组id</param>
        /// <exception cref="T:System.ArgumentException">id格式错误</exception>
        Task CountPresentationGradeAsync(long seminarId, IList<Topic> topicList);

        /// <summary>
        ///     定时器方法:讨论课结束后计算本次讨论课得分.
        ///     @author qinlingyun
        ///     条件: 讨论课已结束，展示得分已算出  *GradeService
        /// </summary>
        /// <param name="seminarId">讨论课id</param>
        /// <param name="seminarGroupList">讨论课小组列表</param>
        /// <exception cref="T:System.ArgumentException">id格式错误</exception>
        Task CountGroupGradeBySerminarIdAsync(long seminarId, IList<SeminarGroup> seminarGroupList);
    }
}