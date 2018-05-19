using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;

namespace Xmu.Crms.Services.Insomnia
{
    public class TopicService : Orleans.Grain, ITopicService
    {
        private readonly CrmsContext _db;

        public TopicService(CrmsContext db) => _db = db;

        /// <inheritdoc />
        public async Task<Topic> GetTopicByTopicIdAsync(long topicId) => await _db.Topic.FindAsync(topicId) ?? throw new TopicNotFoundException();

        /// <inheritdoc />
        public async Task UpdateTopicByTopicIdAsync(long topicId, Topic topic)
        {
            var top = await GetTopicByTopicIdAsync(topicId);
            top.Description = topic.Description;
            top.GroupNumberLimit = topic.GroupNumberLimit;
            top.GroupStudentLimit = topic.GroupStudentLimit;
            top.Serial = topic.Serial ?? top.Serial;
            await _db.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task DeleteTopicByTopicIdAsync(long topicId)
        {
            _db.Remove(await GetTopicByTopicIdAsync(topicId));
            await _db.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<IList<Topic>> ListTopicBySeminarIdAsync(long seminarId)
        {
            var sem = _db.Seminar.Find(seminarId) ?? throw new SeminarNotFoundException();
            return await _db.Topic.Include(t => t.Seminar).Where(t => t.Seminar == sem).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<long> InsertTopicBySeminarIdAsync(long seminarId, Topic topic)
        {
            var sem = _db.Seminar.Find(seminarId) ?? throw new SeminarNotFoundException();
            topic.Seminar = sem;
            topic.Serial = topic.Serial ?? Encoding.ASCII.GetChars(new[]
            {
                (byte) (_db.Topic.Count(t => t.SeminarId == seminarId) + Encoding.ASCII.GetBytes("A")[0])
            })[0].ToString();
            var ent = _db.Topic.Add(topic);
            await _db.SaveChangesAsync();
            return ent.Entity.Id;
        }

        /// <inheritdoc />
        public async Task DeleteSeminarGroupTopicByIdAsync(long groupId, long topicId)
        {
            var top = await GetTopicByTopicIdAsync(topicId);
            var grp = _db.SeminarGroup.Find(groupId) ?? throw new GroupNotFoundException();
            _db.SeminarGroupTopic.RemoveRange(_db.SeminarGroupTopic.Include(s => s.Topic).Include(s => s.SeminarGroup)
                .Where(sg => sg.SeminarGroup == grp && sg.Topic == top));
            await _db.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task DeleteSeminarGroupTopicByTopicIdAsync(long topicId)
        {
            var top = await GetTopicByTopicIdAsync(topicId);
            _db.SeminarGroupTopic.RemoveRange(_db.SeminarGroupTopic.Include(s => s.Topic).Where(sg => sg.Topic == top));
            await _db.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<SeminarGroupTopic> GetSeminarGroupTopicByIdAsync(long topicId, long groupId)
        {
            var top = await GetTopicByTopicIdAsync(topicId);
            var grp = _db.SeminarGroup.Find(groupId) ?? throw new GroupNotFoundException();
            return await _db.SeminarGroupTopic.Include(s => s.Topic).Include(s => s.SeminarGroup)
                .SingleOrDefaultAsync(sg => sg.SeminarGroup == grp && sg.Topic == top);
        }

        /// <inheritdoc />
        public async Task<List<SeminarGroupTopic>> ListSeminarGroupTopicByGroupIdAsync(long groupId)
        {
            var grp = _db.SeminarGroup.Find(groupId) ?? throw new GroupNotFoundException();
            return await _db.SeminarGroupTopic.Include(s => s.Topic).Include(s => s.SeminarGroup)
                .Where(sg => sg.SeminarGroup == grp).ToListAsync();
        }

        /// <inheritdoc />
        public async Task DeleteTopicBySeminarIdAsync(long seminarId)
        {
            _db.RemoveRange(ListTopicBySeminarIdAsync(seminarId));
            await _db.SaveChangesAsync();
        }
    }
}