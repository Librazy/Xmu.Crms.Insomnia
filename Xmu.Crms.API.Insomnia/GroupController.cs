﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public class GroupController : Controller
    {
        private readonly IFixGroupService _fixGroupService;
        private readonly IGradeService _gradeService;
        private readonly ISeminarGroupService _seminarGroupService;
        private readonly ITopicService _topicService;

        public GroupController(
            IClusterClient client,
            JwtHeader header)
        {
            _fixGroupService = client.GetGrain<IFixGroupService>(0);
            _seminarGroupService = client.GetGrain<ISeminarGroupService>(0);
            _topicService = client.GetGrain<ITopicService>(0);
            _gradeService = client.GetGrain<IGradeService>(0);
        }

        [HttpGet("/group/{groupId:long}")]
        public async Task<IActionResult> GetGroupById([FromRoute] long groupId, [FromQuery] bool embedGrade = false)
        {
            try
            {
                var group = await _seminarGroupService.GetSeminarGroupByGroupIdAsync(groupId);
                var members = await _seminarGroupService.ListSeminarGroupMemberByGroupIdAsync(groupId);
                var topics = await _topicService.ListSeminarGroupTopicByGroupIdAsync(groupId);
                if (!embedGrade)
                {
                    return Json(new
                    {
                        id = group.Id,
                        name = group.Id + "组",
                        leader = new
                        {
                            id = group.Leader.Id,
                            name = group.Leader.Name
                        },
                        members = members.Select(m => new
                        {
                            id = m.Id,
                            name = m.Name
                        }),
                        topics = topics.Select(t => new
                        {
                            id = t.Topic.Id,
                            name = t.Topic.Name
                        }),
                        report = group.Report
                    });
                }

                return Json(new
                {
                    id = group.Id,
                    name = group.Id + "组",
                    leader = new
                    {
                        id = group.Leader.Id,
                        name = group.Leader.Name
                    },
                    members = members.Select(m => new
                    {
                        id = m.Id,
                        name = m.Name
                    }),
                    topics = topics.Select(t => new
                    {
                        id = t.Topic.Id,
                        name = t.Topic.Name
                    }),
                    report = group.Report,
                    grade = new
                    {
                        presentationGrade = _topicService.ListSeminarGroupTopicByGroupIdAsync(groupId).Result.Select(
                            p => new
                            {
                                id = p.Id,
                                grade = p.PresentationGrade
                            }),
                        reportGrade = group.ReportGrade,
                        grade = group.FinalGrade
                    }
                });
            }
            catch (GroupNotFoundException)
            {
                return StatusCode(404, new {msg = "未找到小组"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "组号格式错误"});
            }
        }

        /*
         * 没有找到相应的修改seminarGroup的方法，修改了SeminarGroup变为FixedGroup
         */
        [HttpPut("/group/{groupId:long}")]
        public async Task<IActionResult> UpdateGroupById([FromRoute] long groupId,
            [FromBody] /*SeminarGroup*/FixGroup updated)
        {
            try
            {
                if (User.Type() != Type.Teacher)
                {
                    return StatusCode(403, new {msg = "权限不足"});
                }

                await _fixGroupService.UpdateFixGroupByGroupIdAsync(groupId, updated);
                return NoContent();
            }
            catch (GroupNotFoundException)
            {
                return StatusCode(404, new {msg = "没有找到组"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "组号格式错误"});
            }
        }

        [HttpPost("/group/{groupId:long}/topic")]
        public async Task<IActionResult> SelectTopic([FromRoute] long groupId, [FromBody] Topic selected)
        {
            try
            {
                if (User.Type() != Type.Student)
                {
                    return StatusCode(403, new {msg = "权限不足"});
                }

                await _seminarGroupService.InsertTopicByGroupIdAsync(groupId, selected.Id);
                return Created($"/group/{groupId}/topic/{selected.Id}",
                    new Dictionary<string, string> {["url"] = $" /group/{groupId}/topic/{selected.Id}"});
            }
            catch (GroupNotFoundException)
            {
                return StatusCode(404, new {msg = "没有找到该课程"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "组号格式错误"});
            }
        }

        [HttpDelete("/group/{groupId:long}/topic/{topicId:long}")]
        public async Task<IActionResult> DeselectTopic([FromRoute] long groupId, [FromRoute] long topicId)
        {
            try
            {
                if (User.Type() != Type.Student)
                {
                    return StatusCode(403, new {msg = "权限不足"});
                }

                await _topicService.DeleteSeminarGroupTopicByIdAsync(groupId, topicId);
                return NoContent();
            }
            catch (GroupNotFoundException)
            {
                return StatusCode(404, new {msg = "没有找到该课程"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "组号格式错误"});
            }
        }

        [HttpGet("/group/{groupId:long}/grade")]
        public async Task<IActionResult> GetGradeByGroupId([FromRoute] long groupId)
        {
            try
            {
                var group = await _seminarGroupService.GetSeminarGroupByGroupIdAsync(groupId);
                var pGradeTopics = await _topicService.ListSeminarGroupTopicByGroupIdAsync(groupId);
                return Json(new
                {
                    presentationGrade = pGradeTopics.Select(p => new
                    {
                        id = p.Id,
                        grade = p.PresentationGrade
                    }),
                    reportGrade = group.ReportGrade,
                    grade = group.FinalGrade
                });
            }
            catch (GroupNotFoundException)
            {
                return StatusCode(404, new {msg = "没有找到该课程"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "组号格式错误"});
            }
        }

        [HttpPut("/group/{groupId:long}/grade/report")]
        public async Task<IActionResult> UpdateGradeByGroupId([FromRoute] long groupId,
            [FromBody] StudentScoreGroup updated)
        {
            try
            {
                if (User.Type() != Type.Teacher)
                {
                    return StatusCode(403, new {msg = "权限不足"});
                }

                if (updated.Grade != null)
                {
                    await _gradeService.UpdateGroupByGroupIdAsync(groupId, (int) updated.Grade);
                }

                return NoContent();
            }
            catch (GroupNotFoundException)
            {
                return StatusCode(404, new {msg = "没有找到该课程"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "组号格式错误"});
            }
        }

        [HttpPut("/group/{groupId:long}/grade/presentation/{studentId:long}")]
        public async Task<IActionResult> SubmitStudentGradeByGroupId([FromBody] long groupId, [FromBody] long studentId,
            [FromBody] StudentScoreGroup updated)
        {
            try
            {
                if (User.Type() != Type.Student)
                {
                    return StatusCode(403, new {msg = "权限不足"});
                }

                if (updated.Grade == null)
                {
                    return NoContent();
                }

                await _gradeService.InsertGroupGradeByUserIdAsync(updated.SeminarGroupTopic.Topic.Id,
                    updated.Student.Id,
                    groupId, (int) updated.Grade);
                return NoContent();
            }
            catch (GroupNotFoundException)
            {
                return StatusCode(404, new {msg = "没有找到该课程"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "组号格式错误"});
            }
        }
    }
}