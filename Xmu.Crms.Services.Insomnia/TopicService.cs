using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;

namespace Xmu.Crms.Services.Insomnia
{
    public class TopicService : ITopicService
    {
        private readonly CrmsContext _db;

        public TopicService(CrmsContext db) => _db = db;

        /// <inheritdoc />
        public Topic GetTopicByTopicId(long topicId) => _db.Topic.Find(topicId) ?? throw new TopicNotFoundException();

        /// <inheritdoc />
        public void UpdateTopicByTopicId(long topicId, Topic topic)
        {
            var top = GetTopicByTopicId(topicId);
            top.Description = topic.Description;
            top.GroupNumberLimit = topic.GroupNumberLimit;
            top.GroupStudentLimit = topic.GroupStudentLimit;
            top.Serial = topic.Serial ?? top.Serial;
            _db.SaveChanges();
        }

        /// <inheritdoc />
        public void DeleteTopicByTopicId(long topicId)
        {
            _db.Remove(GetTopicByTopicId(topicId));
        }

        /// <inheritdoc />
        public IList<Topic> ListTopicBySeminarId(long seminarId)
        {
            var sem = _db.Seminar.Find(seminarId) ?? throw new SeminarNotFoundException();
            return _db.Topic.Include(t => t.Seminar).Where(t => t.Seminar == sem).ToList();
        }

        /// <inheritdoc />
        public long InsertTopicBySeminarId(long seminarId, Topic topic)
        {
            var sem = _db.Seminar.Find(seminarId) ?? throw new SeminarNotFoundException();
            topic.Seminar = sem;
            topic.Serial = topic.Serial ?? Encoding.ASCII.GetChars(new[]
            {
                (byte) (_db.Topic.Count(t => t.SeminarId == seminarId) + Encoding.ASCII.GetBytes("A")[0])
            })[0].ToString();
            var ent = _db.Topic.Add(topic);
            _db.SaveChanges();
            return ent.Entity.Id;
        }

        /// <inheritdoc />
        public void DeleteSeminarGroupTopicById(long groupId, long topicId)
        {
            var top = GetTopicByTopicId(topicId);
            var grp = _db.SeminarGroup.Find(groupId) ?? throw new GroupNotFoundException();
            _db.SeminarGroupTopic.RemoveRange(_db.SeminarGroupTopic.Include(s => s.Topic).Include(s => s.SeminarGroup)
                .Where(sg => sg.SeminarGroup == grp && sg.Topic == top));
            _db.SaveChanges();
        }

        /// <inheritdoc />
        public void DeleteSeminarGroupTopicByTopicId(long topicId)
        {
            var top = GetTopicByTopicId(topicId);
            _db.SeminarGroupTopic.RemoveRange(_db.SeminarGroupTopic.Include(s => s.Topic).Where(sg => sg.Topic == top));
            _db.SaveChanges();
        }

        /// <inheritdoc />
        public SeminarGroupTopic GetSeminarGroupTopicById(long topicId, long groupId)
        {
            var top = GetTopicByTopicId(topicId);
            var grp = _db.SeminarGroup.Find(groupId) ?? throw new GroupNotFoundException();
            return _db.SeminarGroupTopic.Include(s => s.Topic).Include(s => s.SeminarGroup)
                .SingleOrDefault(sg => sg.SeminarGroup == grp && sg.Topic == top);
        }

        /// <inheritdoc />
        public List<SeminarGroupTopic> ListSeminarGroupTopicByGroupId(long groupId)
        {
            var grp = _db.SeminarGroup.Find(groupId) ?? throw new GroupNotFoundException();
            return _db.SeminarGroupTopic.Include(s => s.Topic).Include(s => s.SeminarGroup)
                .Where(sg => sg.SeminarGroup == grp).ToList();
        }

        /// <inheritdoc />
        public void DeleteTopicBySeminarId(long seminarId)
        {
            _db.RemoveRange(ListTopicBySeminarId(seminarId));
            _db.SaveChanges();
        }
    }
}