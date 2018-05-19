using System.Collections.Generic;
using System.Threading.Tasks;
using Xmu.Crms.Shared.Models;

namespace Xmu.Crms.Shared.Service
{
    /// <summary>
    ///     @author ModuleStandardGroup/zhouzhongjun
    ///     @version 2.00
    /// </summary>
    public interface ISeminarGroupService : Orleans.IGrainWithIntegerKey
    {
        /// <summary>
        ///     按seminarGroupId删除SeminarGroupMember信息.
        ///     @author zhouzhongjun
        /// </summary>
        /// <param name="seminarGroupId">讨论课小组Id</param>
        Task DeleteSeminarGroupMemberBySeminarGroupIdAsync(long seminarGroupId);

        /// <summary>
        ///     将学生加入讨论课小组.
        ///     @author YeHongjie
        /// </summary>
        /// <param name="userId">学生的id</param>
        /// <param name="groupId">要加入讨论课小组的id</param>
        /// <returns>long 该条记录的id</returns>
        /// <exception cref="T:System.ArgumentException">id格式错误</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.GroupNotFoundException">未找到小组</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.UserNotFoundException">不存在该学生</exception>
        /// <exception cref="T:System.InvalidOperationException">待添加学生已经在小组里了</exception>
        Task<long> InsertSeminarGroupMemberByIdAsync(long userId, long groupId);

        /// <summary>
        ///     查询讨论课小组成员.
        ///     @author YeHongjie
        /// </summary>
        /// <param name="groupId">要查询的讨论课小组id</param>
        /// <returns>List 讨论课小组成员信息</returns>
        /// <exception cref="T:System.ArgumentException">id格式错误</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.GroupNotFoundException">未找到小组</exception>
        Task<IList<UserInfo>> ListSeminarGroupMemberByGroupIdAsync(long groupId);

        /// <summary>
        ///     获取某学生所有的讨论课小组.
        ///     @author qinlingyun
        /// </summary>
        /// <param name="userId">学生id</param>
        /// <returns>list 讨论课小组列表</returns>
        /// <exception cref="T:System.ArgumentException">id格式错误</exception>
        Task<IList<SeminarGroup>> ListSeminarGroupIdByStudentIdAsync(long userId);

        /// <summary>
        ///     查询讨论课小组队长id.
        ///     @author YeHongjie
        /// </summary>
        /// <param name="groupId">要查询的讨论课小组id</param>
        /// <returns>leaderId 讨论课小组队长id</returns>
        /// <exception cref="T:System.ArgumentException">id格式错误</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.GroupNotFoundException">未找到小组</exception>
        Task<long> GetSeminarGroupLeaderByGroupIdAsync(long groupId);

        /// <summary>
        ///     按seminarId获取SeminarGroup.
        ///     @author zhouzhongjun
        /// </summary>
        /// <param name="seminarId">课程Id</param>
        /// <returns>讨论课小组列表</returns>
        /// <exception cref="T:System.ArgumentException">id格式错误</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.SeminarNotFoundException">未找到小组</exception>
        Task<IList<SeminarGroup>> ListSeminarGroupBySeminarIdAsync(long seminarId);

        /// <summary>
        ///     按seminarId删除讨论课小组信息.
        ///     @author zhouzhongjun
        ///     根据seminarId获得SeminarGroup，然后根据SeminarGroupId删除SeminarGroupMember信息，最后再删除SeminarGroup信息
        /// </summary>
        /// <param name="seminarId">讨论课Id</param>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ISeminarGroupService.ListSeminarGroupBySeminarIdAsync(System.Int64)" />
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ISeminarGroupService.DeleteSeminarGroupMemberBySeminarGroupIdAsync(System.Int64)" />
        /// <exception cref="T:System.ArgumentException">id格式错误</exception>
        Task DeleteSeminarGroupBySeminarIdAsync(long seminarId);

        /// <summary>
        ///     创建讨论课小组.
        ///     @author YeHongjie
        /// </summary>
        /// <param name="seminarId">讨论课的id</param>
        /// <param name="classId"></param>
        /// <param name="seminarGroup">小组信息</param>
        /// <returns>long 返回该小组的id</returns>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ISeminarGroupService.InsertSeminarGroupMemberByIdAsync(System.Int64,System.Int64)" />
        /// <exception cref="T:System.ArgumentException">id格式错误</exception>
        Task<long> InsertSeminarGroupBySeminarIdAsync(long seminarId, long classId, SeminarGroup seminarGroup);

        /// <summary>
        ///     创建小组成员信息.
        /// </summary>
        /// <param name="groupId">小组的id</param>
        /// <param name="seminarGroupMember">小组成员信息</param>
        /// <returns>long 返回该小组成员表的id</returns>
        Task<long> InsertSeminarGroupMemberByGroupIdAsync(long groupId, SeminarGroupMember seminarGroupMember);

        /// <summary>
        ///     删除讨论课小组.
        ///     @author YeHongjie
        /// </summary>
        /// <param name="seminarGroupId">讨论课小组的id</param>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ISeminarGroupService.DeleteSeminarGroupMemberBySeminarGroupIdAsync(System.Int64)" />
        Task DeleteSeminarGroupByGroupIdAsync(long seminarGroupId);

        /// <summary>
        ///     查询讨论课小组.
        ///     @author YeHongjie
        ///     按照id查询某一讨论课小组的信息（包括成员）
        /// </summary>
        /// <param name="groupId">小组的id</param>
        /// <returns>seminarGroup 讨论课小组对象，若未找到相关小组返回空(null)</returns>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ISeminarGroupService.ListSeminarGroupMemberByGroupIdAsync(System.Int64)" />
        /// <exception cref="T:System.ArgumentException">id格式错误</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.GroupNotFoundException">未找到小组</exception>
        Task<SeminarGroup> GetSeminarGroupByGroupIdAsync(long groupId);

        /// <summary>
        ///     获取学生所在讨论课队长.
        ///     @author YeHongjie
        /// </summary>
        /// <param name="userId">用户的id</param>
        /// <param name="seminarId">讨论课id</param>
        /// <returns>long 讨论课小组的队长id，若未找到相关小组队长返回空(null)</returns>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ISeminarGroupService.GetSeminarGroupByIdAsync(System.Int64,System.Int64)" />
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ISeminarGroupService.GetSeminarGroupLeaderByGroupIdAsync(System.Int64)" />
        /// <exception cref="T:System.ArgumentException">id格式错误</exception>
        Task<long> GetSeminarGroupLeaderByIdAsync(long userId, long seminarId);

        /// <summary>
        ///     定时器方法：自动分组.
        ///     @author YeHongjie
        /// </summary>
        /// 根据讨论课id和班级id，对签到的学生进行自动分组
        /// <param name="seminarId">讨论课的id</param>
        /// <param name="classId">班级的id</param>
        /// <returns>Boolean 自动分组成功返回true，否则返回false</returns>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IUserService.ListAttendanceByIdAsync(System.Int64,System.Int64)" />
        /// <exception cref="T:System.ArgumentException">id格式错误</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.SeminarNotFoundException">未找到讨论课</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.ClassNotFoundException">未找到班级</exception>
        Task AutomaticallyGroupingAsync(long seminarId, long classId);

        /// <summary>
        ///     根据讨论课Id及用户id，获得该用户所在的讨论课的小组的信息.
        /// </summary>
        /// <param name="seminarId">(讨论课的id)</param>
        /// <param name="userId"></param>
        /// <returns>SeminarGroup Group的相关信息</returns>
        /// <exception cref="T:System.ArgumentException">id格式错误</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.GroupNotFoundException">未找到小组</exception>
        Task<SeminarGroup> GetSeminarGroupByIdAsync(long seminarId, long userId);

        /// <summary>
        ///     根据话题Id获得选择该话题的所有小组的信息.
        /// </summary>
        /// <param name="topicId"></param>
        /// <returns>List&lt;GroupBO&gt;所有选择该话题的所有group的信息</returns>
        /// <exception cref="T:System.ArgumentException">id格式错误</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.GroupNotFoundException">未找到小组</exception>
        Task<IList<SeminarGroup>> ListGroupByTopicIdAsync(long topicId);

        /// <summary>
        ///     小组按id选择话题.
        ///     @author heqi
        /// </summary>
        /// <param name="groupId">小组id</param>
        /// <param name="topicId">话题id</param>
        /// <exception cref="T:System.ArgumentException">id格式错误</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.GroupNotFoundException">未找到小组</exception>
        Task InsertTopicByGroupIdAsync(long groupId, long topicId);

        /// <summary>
        ///     成为组长.
        ///     同学按小组id和自身id成为组长
        /// </summary>
        /// <param name="groupId">小组id</param>
        /// <param name="userId">学生id</param>
        /// <exception cref="T:System.ArgumentException">id格式错误</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.UserNotFoundException">不存在该学生</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.GroupNotFoundException">未找到小组</exception>
        /// <exception cref="T:System.InvalidOperationException">已经有组长了</exception>
        Task AssignLeaderByIdAsync(long groupId, long userId);

        /// <summary>
        ///     组长辞职.
        ///     同学按小组id和自身id,辞掉组长职位
        /// </summary>
        /// <param name="groupId">小组id</param>
        /// <param name="userId">学生id</param>
        /// <exception cref="T:System.ArgumentException">id格式错误</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.UserNotFoundException">不存在该学生</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.GroupNotFoundException">未找到小组</exception>
        /// <exception cref="T:System.InvalidOperationException">学生不是组长</exception>
        Task ResignLeaderByIdAsync(long groupId, long userId);
    }
}