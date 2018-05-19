using System.Collections.Generic;
using System.Threading.Tasks;
using Xmu.Crms.Shared.Models;

namespace Xmu.Crms.Shared.Service
{
    /// <summary>
    ///     @author Aixing ZhouZhongjun
    ///     @version 2.00
    /// </summary>
    public interface ITopicService : Orleans.IGrainWithGuidKey
    {
        /// <summary>
        ///     按topicId获取topic.
        ///     @author aixing
        /// </summary>
        /// <param name="topicId">要获取的topic的topicId</param>
        /// <returns>该topic</returns>
        /// <exception cref="T:System.ArgumentException">Id格式错误时抛出</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.TopicNotFoundException">无此小组或Id错误</exception>
        Task<Topic> GetTopicByTopicIdAsync(long topicId);

        /// <summary>
        ///     根据topicId修改topic.
        ///     @author aixing
        /// </summary>
        /// <param name="topicId">讨论课的ID</param>
        /// <param name="topic">修改后的讨论课</param>
        /// <exception cref="T:System.ArgumentException">Id格式错误时抛出</exception>
        /// <exception cref="T:Xmu.Crms.Shared.Exceptions.TopicNotFoundException">无此小组或Id错误</exception>
        Task UpdateTopicByTopicIdAsync(long topicId, Topic topic);

        /// <summary>
        ///     删除topic.
        /// </summary>
        /// <param name="topicId">要删除的topic的topicId</param>
        /// <exception cref="T:System.ArgumentException">Id格式错误时抛出</exception>
        Task DeleteTopicByTopicIdAsync(long topicId);

        /// <summary>
        ///     按seminarId获取Topic.
        ///     @author zhouzhongjun
        /// </summary>
        /// <param name="seminarId">课程Id</param>
        /// <returns>null</returns>
        /// <exception cref="T:System.ArgumentException">Id格式错误时抛出</exception>
        Task<IList<Topic>> ListTopicBySeminarIdAsync(long seminarId);


        /// <summary>
        ///     根据讨论课Id和topic信息创建一个话题.
        ///     @author aixing
        /// </summary>
        /// <param name="seminarId">话题所属讨论课的Id</param>
        /// <param name="topic">话题</param>
        /// <returns>新建话题后给topic分配的Id</returns>
        /// <exception cref="T:System.ArgumentException">Id格式错误时抛出</exception>
        Task<long> InsertTopicBySeminarIdAsync(long seminarId, Topic topic);

        /// <summary>
        ///     小组取消选择话题.
        ///     @author zhouzhongjun
        ///     删除seminar_group_topic表的记录
        /// </summary>
        /// <param name="groupId">小组Id</param>
        /// <param name="topicId">话题Id</param>
        /// <exception cref="T:System.ArgumentException">groupId格式错误或topicId格式错误时抛出</exception>
        Task DeleteSeminarGroupTopicByIdAsync(long groupId, long topicId);

        /// <summary>
        ///     按topicId删除SeminarGroupTopic表信息.
        ///     @author zhouzhongjun
        /// </summary>
        /// <param name="topicId">讨论课Id</param>
        /// <exception cref="T:System.ArgumentException">topicId格式错误</exception>
        Task DeleteSeminarGroupTopicByTopicIdAsync(long topicId);

        /// <summary>
        ///     按话题id和小组id获取讨论课小组选题信息
        /// </summary>
        /// <param name="topicId">讨论课Id</param>
        /// <param name="groupId">小组Id</param>
        /// <returns>seminarGroupTopic 讨论课小组选题信息</returns>
        /// <exception cref="T:System.ArgumentException">seminarId格式错误</exception>
        Task<SeminarGroupTopic> GetSeminarGroupTopicByIdAsync(long topicId, long groupId);


        /// 根据小组id获取该小组该堂讨论课所有选题信息
        /// 根据小组id获取该小组该堂讨论课所有选题信息
        /// @param groupId
        /// @return list 该小组该堂讨论课选题列表
        /// @exception IllegalArgumentException groupId格式错误
        Task<List<SeminarGroupTopic>> ListSeminarGroupTopicByGroupIdAsync(long groupId);


        /// <summary>
        ///     按seminarId删除话题.
        ///     @author zhouzhongjun
        ///     根据seminarId获得topic信息，然后再根据topic删除seninargrouptopic信息和StudentScoreGroup信息，最后再根据删除topic信息
        /// </summary>
        /// <param name="seminarId">讨论课Id</param>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ITopicService.ListTopicBySeminarIdAsync(System.Int64)" />
        /// <seealso cref="M:Xmu.Crms.Shared.Service.ITopicService.DeleteSeminarGroupTopicByTopicIdAsync(System.Int64)" />
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IGradeService.DeleteStudentScoreGroupByTopicIdAsync(System.Int64)" />
        /// <exception cref="T:System.ArgumentException">seminarId格式错误</exception>
        Task DeleteTopicBySeminarIdAsync(long seminarId);
    }
}