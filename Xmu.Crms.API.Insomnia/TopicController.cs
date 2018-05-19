using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;

namespace Xmu.Crms.Insomnia
{
    [Route("")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TopicController : Controller
    {
        private readonly ISeminarGroupService _seminarGroupService;
        private readonly ITopicService _topicService;

        public TopicController(IClusterClient client)
        {
            _topicService = client.GetGrain<ITopicService>(0);
            _seminarGroupService = client.GetGrain<ISeminarGroupService>(0);
        }

        [HttpGet("/topic/{topicId:long}")]
        public async Task<IActionResult> GetTopicById([FromRoute] long topicId)
        {
            try
            {
                var t = await _topicService.GetTopicByTopicIdAsync(topicId);
                return Json(new
                {
                    id = t.Id,
                    serial = t.Serial,
                    name = t.Name,
                    description = t.Description,
                    groupLimit = t.GroupNumberLimit,
                    groupMemberLimit = t.GroupStudentLimit
                });
            }
            catch (TopicNotFoundException)
            {
                return StatusCode(404, new {msg = "话题不存在"});
            }
        }

        [HttpDelete("/topic/{topicId:long}")]
        public async Task<IActionResult> DeleteTopicById([FromRoute] long topicId)
        {
            if (User.Type() != Type.Teacher)
            {
                return StatusCode(403, new {msg = "权限不足"});
            }

            try
            {
                await _topicService.DeleteTopicByTopicIdAsync(topicId);
                return NoContent();
            }
            catch (TopicNotFoundException)
            {
                return StatusCode(404, new {msg = "话题不存在"});
            }
        }

        [HttpPut("/topic/{topicId:long}")]
        public async Task<IActionResult> UpdateTopicById([FromRoute] long topicId, [FromBody] Topic updated)
        {
            if (User.Type() != Type.Teacher)
            {
                return StatusCode(403, new {msg = "权限不足"});
            }

            try
            {
                await _topicService.UpdateTopicByTopicIdAsync(topicId, updated);
                return NoContent();
            }
            catch (TopicNotFoundException)
            {
                return StatusCode(404, new {msg = "话题不存在"});
            }
        }

        [HttpGet("/topic/{topicId:long}/group")]
        public async Task<IActionResult> GetGroupsByTopicId([FromRoute] long topicId)
        {
            try
            {
                return Json((await _seminarGroupService.ListGroupByTopicIdAsync(topicId))
                    .Select(s => new {id = s.Id, name = s.Id + "组"}));
            }
            catch (TopicNotFoundException)
            {
                return StatusCode(404, new {msg = "话题不存在"});
            }
        }
    }
}