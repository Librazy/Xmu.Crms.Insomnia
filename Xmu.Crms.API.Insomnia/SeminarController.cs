using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orleans;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using Type = Xmu.Crms.Shared.Models.Type;

namespace Xmu.Crms.Insomnia
{
    [Route("")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SeminarController : Controller
    {
        private readonly CrmsContext _db;
        private readonly ISeminarGroupService _seminargroupService;
        private readonly ISeminarService _seminarService;
        private readonly ITopicService _topicService;
        private readonly IUserService _userService;

        public SeminarController(IClusterClient client, CrmsContext db)
        {
            _seminarService = client.GetGrain<ISeminarService>(0);
            _topicService = client.GetGrain<ITopicService>(0);
            _seminargroupService = client.GetGrain<ISeminarGroupService>(0);
            _userService = client.GetGrain<IUserService>(0);
            _db = db;
        }

        [HttpGet("/seminar/{seminarId:long}")]
        public async Task<IActionResult> GetSeminarById([FromRoute] long seminarId)
        {
            try
            {
                var sem = await _seminarService.GetSeminarBySeminarIdAsync(seminarId);
                return Json(new
                {
                    id = sem.Id,
                    name = sem.Name,
                    description = sem.Description,
                    startTime = sem.StartTime.ToString("yyyy-MM-dd"),
                    endTime = sem.EndTime.ToString("yyyy-MM-dd")
                });
            }
            catch (SeminarNotFoundException)
            {
                return StatusCode(404, new {msg = "讨论课不存在"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "讨论课ID输入格式有误"});
            }
        }

        [HttpPut("/seminar/{seminarId:long}")]
        public IActionResult UpdateSeminarById([FromRoute] long seminarId, [FromBody] Seminar updated)
        {
            if (User.Type() != Type.Teacher)
            {
                return StatusCode(403, new {msg = "权限不足"});
            }

            try
            {
                _seminarService.UpdateSeminarBySeminarIdAsync(seminarId, updated);
                return NoContent();
            }
            catch (SeminarNotFoundException)
            {
                return StatusCode(404, new {msg = "讨论课不存在"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "讨论课ID输入格式有误"});
            }
        }

        [HttpDelete("/seminar/{seminarId:long}")]
        public async Task<IActionResult> DeleteSeminarById([FromRoute] long seminarId)
        {
            if (User.Type() != Type.Teacher)
            {
                return StatusCode(403, new {msg = "权限不足"});
            }

            try
            {
                await _seminarService.DeleteSeminarBySeminarIdAsync(seminarId);
                return NoContent();
            }
            catch (SeminarNotFoundException)
            {
                return StatusCode(404, new {msg = "讨论课不存在"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "讨论课ID输入格式有误"});
            }
        }

        //groupLeft未加
        [HttpGet("/seminar/{seminarId:long}/topic")]
        public async Task<IActionResult> GetTopicsBySeminarId([FromRoute] long seminarId)
        {
            try
            {
                var topics = await _topicService.ListTopicBySeminarIdAsync(seminarId);
                return Json(topics.Select(t => new
                {
                    id = t.Id,
                    serial = t.Serial,
                    name = t.Name,
                    description = t.Description,
                    groupLimit = t.GroupNumberLimit,
                    groupMemberLimit = t.GroupStudentLimit
                }));
            }
            catch (SeminarNotFoundException)
            {
                return StatusCode(404, new {msg = "讨论课不存在"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "话题ID输入格式有误"});
            }
        }

        [HttpPost("/seminar/{seminarId:long}/topic")]
        public async Task<IActionResult> CreateTopicBySeminarId([FromRoute] long seminarId, [FromBody] Topic newTopic)
        {
            if (User.Type() != Type.Teacher)
            {
                return StatusCode(403, new {msg = "权限不足"});
            }

            var topicid = await _topicService.InsertTopicBySeminarIdAsync(seminarId, newTopic);
            return Created("/topic/" + topicid, newTopic);
        }

        //没有小组成员 和 report
        [HttpGet("/seminar/{seminarId:long}/group")]
        public async Task<IActionResult> GetGroupsBySeminarId([FromRoute] long seminarId)
        {
            try
            {
                var groups = await _seminargroupService.ListSeminarGroupBySeminarIdAsync(seminarId);
                return Json(groups.Select(t => new
                {
                    id = t.Id,
                    name = t.Id + "组"
                }));
            }
            catch (SeminarNotFoundException)
            {
                return StatusCode(404, new {msg = "讨论课不存在"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "讨论课ID输入格式有误"});
            }
        }

        [HttpGet("/seminar/{seminarId:long}/group/my")]
        public async Task<IActionResult> GetStudentGroupBySeminarId([FromRoute] long seminarId)
        {
            if (User.Type() != Type.Student)
            {
                return StatusCode(403, new {msg = "权限不足"});
            }

            try
            {
                var groups = await _seminargroupService.ListSeminarGroupBySeminarIdAsync(seminarId);
                var group = groups.SelectMany(grp => _db.Entry(grp).Collection(gp => gp.SeminarGroupMembers).Query()
                                    .Include(gm => gm.SeminarGroup)
                                    .Where(gm => gm.StudentId == User.Id()).Select(gm => gm.SeminarGroup))
                                .SingleOrDefault(sg => sg.SeminarId == seminarId) ?? throw new GroupNotFoundException();
                var leader = group.Leader ?? await _userService.GetUserByUserIdAsync(group.LeaderId);
                var members = await _seminargroupService.ListSeminarGroupMemberByGroupIdAsync(group.Id);
                var topics = await Task.WhenAll((await _topicService.ListSeminarGroupTopicByGroupIdAsync(group.Id))
                    .Select(gt => _topicService.GetTopicByTopicIdAsync(gt.TopicId)));
                return Json(new
                {
                    id = group.Id,
                    name = group.Id + "组",
                    leader = new
                    {
                        id = leader.Id,
                        name = leader.Name
                    },
                    members = members.Select(u => new {id = u.Id, name = u.Name}),
                    topics = topics.Select(t => new {id = t.Id, name = t.Name})
                });
            }
            catch (SeminarNotFoundException)
            {
                return StatusCode(404, new {msg = "讨论课不存在"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "讨论课ID输入格式有误"});
            }
        }
    }
}