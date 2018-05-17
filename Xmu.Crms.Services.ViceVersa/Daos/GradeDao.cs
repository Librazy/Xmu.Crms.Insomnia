using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;

namespace Xmu.Crms.Services.ViceVersa.Daos
{
    internal class GradeDao : IGradeDao
    {
        private readonly CrmsContext _db;

        // 在构造函数里添加依赖的Service（参考模块标准组的类图）
        public GradeDao(CrmsContext db) => _db = db;

        public void DeleteStudentScoreGroupByTopicId(long topicId)
        {
            using (var scope = _db.Database.BeginTransaction())
            {
                try
                {
                    //查找到所有的seminarGroupTopic
                    //查找到所有的studentScoreGroup//.AsNoTracking()
                    var seminarGroupTopicList = _db.SeminarGroupTopic.Include(u => u.Topic)
                        .Where(u => u.Topic.Id == topicId).ToList();
                    if (seminarGroupTopicList == null)
                    {
                        throw new GroupNotFoundException();
                    }

                    foreach (var seminarGroupTopic in seminarGroupTopicList)
                    {
                        var studentScoreGroupList = _db.StudentScoreGroup.Include(u => u.SeminarGroupTopic)
                            .Where(u => u.SeminarGroupTopic.Id == seminarGroupTopic.Id).ToList();
                        foreach (var studentScoreGroup in studentScoreGroupList)
                        {
                            //将实体附加到对象管理器中
                            _db.StudentScoreGroup.Attach(studentScoreGroup);
                            //删除
                            _db.StudentScoreGroup.Remove(studentScoreGroup);
                        }
                    }

                    _db.SaveChanges();
                }
                catch
                {
                    scope.Rollback();
                    throw;
                }
            }
        }

        public SeminarGroup GetSeminarGroupBySeminarGroupId(long seminarGroupId)
        {
            try
            {
                var group = _db.SeminarGroup.SingleOrDefault(c => c.Id == seminarGroupId);
                if (group == null)
                {
                    throw new GroupNotFoundException();
                }

                return group;
            }
            catch
            {
                throw;
            }
        }

        //不用写，调用其他的
        public List<SeminarGroup> ListSeminarGradeByCourseId(long userId, long courseId) => null;

        //先在seminarGroupTopic 和userinfo service查好，在这里传入实体对象seminarGroupTopic，userInfo
        public void InsertGroupGradeByUserId(SeminarGroupTopic seminarGroupTopic, UserInfo userInfo, int grade)
        {
            using (var scope = _db.Database.BeginTransaction())
            {
                try
                {
                    var ssg = new StudentScoreGroup
                    {
                        Student = userInfo,
                        SeminarGroupTopic = seminarGroupTopic,
                        Grade = grade
                    };
                    _db.StudentScoreGroup.Add(ssg);
                    _db.SaveChanges();
                }
                catch
                {
                    scope.Rollback();
                    throw;
                }
            }
        }

        public void UpdateGroupByGroupId(long seminarGroupId, int grade)
        {
            using (var scope = _db.Database.BeginTransaction())
            {
                try
                {
                    var seminarGroup = _db.SeminarGroup.SingleOrDefault(s => s.Id == seminarGroupId);
                    //如果找不到该组
                    if (seminarGroup == null)
                    {
                        throw new GroupNotFoundException();
                    }

                    //更新报告分
                    seminarGroup.ReportGrade = grade;
                    _db.SaveChanges();
                }
                catch
                {
                    scope.Rollback();
                    throw;
                }
            }
        }

        //在SeminarGroupService调用 IList<SeminarGroup> ListSeminarGroupIdByStudentId(long userId);
        public List<SeminarGroup> ListSeminarGradeByStudentId(long userId) => null;

        //只需要讨论课ID，因为要排序，必须要一起计算
        public void CountPresentationGrade(long seminarId, IList<Topic> topicList)
        {
            using (var scope = _db.Database.BeginTransaction())
            {
                try
                {
                    CalculatePreGradeByTopicId(seminarId, topicList); //计算每个小组每个topic得分
                }
                catch
                {
                    scope.Rollback();
                    throw;
                }
            }
        }

        public void CountGroupGradeBySerminarId(long seminarId, IList<SeminarGroup> seminarGroupList)
        {
            using (var scope = _db.Database.BeginTransaction())
            {
                //根据seminarGroupList中元素，依次计算
                //SeminarGroup实体中保存了ClassInfo实体对象，可以查到成绩计算方法
                var tempTotalGrade = new double[seminarGroupList.Count];
                var tempId = new long[seminarGroupList.Count];
                foreach (var seminarGroup in seminarGroupList)
                {
                    //根据seminarGroupId获得seminarGroupTopicList,计算每一个seminarGroup的展示分数
                    var seminarGroupTopicList = _db.SeminarGroupTopic.Include(u => u.SeminarGroup)
                        .Where(u => u.SeminarGroup.Id == seminarGroup.Id).ToList();
                    if (seminarGroupTopicList == null) //该组没有展示分数
                    {
                        seminarGroupList.Remove(seminarGroup);
                    }

                    int? grade = 0;
                    var number = 0;
                    foreach (var seminarGroupTopic in seminarGroupTopicList)
                    {
                        grade += seminarGroupTopic.PresentationGrade;
                        number++;
                    }

                    try
                    {
                        //更新seminarGroup中的展示成绩
                        var avgPreGrade = grade / number;
                        _db.SeminarGroup.Attach(seminarGroup);
                        seminarGroup.PresentationGrade = avgPreGrade;
                        _db.SaveChanges();
                    }
                    catch
                    {
                        scope.Rollback();
                        throw;
                    }
                }

                for (var i = 0; i < seminarGroupList.Count; i++)
                {
                    tempTotalGrade[i] = (double) (seminarGroupList[i].ClassInfo.PresentationPercentage *
                                                  seminarGroupList[i].PresentationGrade
                                                  + seminarGroupList[i].ClassInfo.ReportPercentage *
                                                  seminarGroupList[i].ReportGrade) / 100;
                    tempId[i] = seminarGroupList[i].Id;
                }

                //排序
                //将小组总成绩从大到小排序
                QuickSort(tempId, tempTotalGrade, 0, seminarGroupList.Count - 1);
                //根据排序和比例计算组数
                var A = Convert.ToInt32(seminarGroupList.Count * seminarGroupList[0].ClassInfo.FivePointPercentage *
                                        0.01);
                var B = Convert.ToInt32(seminarGroupList.Count * seminarGroupList[0].ClassInfo.FourPointPercentage *
                                        0.01);
                var C = Convert.ToInt32(seminarGroupList.Count * seminarGroupList[0].ClassInfo.ThreePointPercentage *
                                        0.01);

                //各小组按比例给分
                for (var i = 0; i < A; i++)
                {
                    try
                    {
                        ////更新报告分
                        //_db.SeminarGroup.Attach(seminarGroupList[i]);
                        //seminarGroupList[i].FinalGrade = 3;
                        //_db.SaveChanges();

                        var seminarGroup = _db.SeminarGroup.SingleOrDefault(s => s.Id == tempId[i]);
                        //如果找不到该组
                        if (seminarGroup == null)
                        {
                            throw new GroupNotFoundException();
                        }

                        //更新报告分
                        seminarGroup.FinalGrade = 5;
                        _db.SaveChanges();
                    }
                    catch
                    {
                        scope.Rollback();
                        throw;
                    }
                }

                for (var i = B; i < B + C; i++)
                {
                    try
                    {
                        var seminarGroup = _db.SeminarGroup.SingleOrDefault(s => s.Id == tempId[i]);
                        //如果找不到该组
                        if (seminarGroup == null)
                        {
                            throw new GroupNotFoundException();
                        }

                        //更新报告分
                        seminarGroup.FinalGrade = 4;
                        _db.SaveChanges();
                    }
                    catch
                    {
                        scope.Rollback();
                        throw;
                    }
                }

                for (var i = B + C; i < seminarGroupList.Count; i++)
                {
                    try
                    {
                        var seminarGroup = _db.SeminarGroup.SingleOrDefault(s => s.Id == tempId[i]);
                        //如果找不到该组
                        if (seminarGroup == null)
                        {
                            throw new GroupNotFoundException();
                        }

                        //更新报告分
                        seminarGroup.FinalGrade = 3;
                        _db.SaveChanges();
                    }
                    catch
                    {
                        scope.Rollback();
                        throw;
                    }
                }
            } //using scope end
        }

        public void Change(long[] idList, double[] gradeList, int i, int j)
        {
            var id = idList[i];
            var grade = gradeList[i];
            idList[i] = idList[j];
            gradeList[i] = gradeList[j];
            idList[j] = id;
            gradeList[j] = grade;
        }

        public void QuickSort(long[] idList, double[] gradeList, int low, int high)
        {
            if (low >= high)
            {
                return;
            }

            var ran = new Random();
            var flag = ran.Next(low, high);
            var id = idList[flag];
            var grade = gradeList[flag];
            var i = low;
            var j = high;
            while (i < j)
            {
                while (gradeList[j] <= grade && i < j)
                {
                    j--;
                }

                if (i < j)
                {
                    Change(idList, gradeList, i, j);
                    i++;
                }

                while (gradeList[j] >= grade && i < j)
                {
                    i++;
                }

                if (i < j)
                {
                    Change(idList, gradeList, i, j);
                    j--;
                }
            }

            gradeList[i] = grade;
            idList[i] = id;
            QuickSort(idList, gradeList, low, i - 1);
            QuickSort(idList, gradeList, i + 1, high);
        }

        public void CalculatePreGradeByTopicId(long seminarId, IList<Topic> topicList)
        {
            foreach (var topic in topicList)
            {
                //通过seminarGroupId获得List<SeminarGroupTopic>  
                //通过seminarGrouptopicId获得List<StudentScoreGroup>
                long[] idList;
                double[] gradeList;

                //获取选择该topic的所有小组
                var seminarGroupTopicList = _db.SeminarGroupTopic.Include(u => u.SeminarGroup).Include(u => u.Topic)
                    .Where(u => u.Topic.Id == topic.Id).ToList();
                if (seminarGroupTopicList == null)
                {
                    throw new GroupNotFoundException();
                }

                if (seminarGroupTopicList != null)
                {
                    idList = new long[seminarGroupTopicList.Count];
                    gradeList = new double[seminarGroupTopicList.Count];

                    var groupNumber = 0;

                    //计算一个小组的所有学生打分情况
                    foreach (var i in seminarGroupTopicList)
                    {
                        //List<StudentScoreGroup> studentScoreList = new List<StudentScoreGroup>();
                        //获取学生打分列表
                        var studentScoreList =
                            _db.StudentScoreGroup.Where(u => u.SeminarGroupTopic.Id == i.Id).ToList();
                        if (studentScoreList == null) //该组没有被打分
                        {
                            seminarGroupTopicList.Remove(i);
                        }

                        int? grade = 0;
                        var k = 0;
                        foreach (var g in studentScoreList)
                        {
                            grade += g.Grade;
                            k++;
                        }

                        var avg = (double) grade / k;

                        //将小组该讨论课平均分和Id保存
                        idList[groupNumber] = i.Id;
                        gradeList[groupNumber] = avg;
                        groupNumber++;
                    }

                    //将小组成绩从大到小排序
                    QuickSort(idList, gradeList, 0, groupNumber - 1);

                    Seminar seminar;
                    ClassInfo classInfo;
                    try
                    {
                        seminar = _db.Seminar.Include(u => u.Course).Where(u => u.Id == seminarId).SingleOrDefault();
                        if (seminar == null)
                        {
                            throw new SeminarNotFoundException();
                        }

                        classInfo = _db.ClassInfo.Where(u => u.Id == seminar.Course.Id).SingleOrDefault();
                        if (classInfo == null)
                        {
                            throw new ClassNotFoundException();
                        }
                    }
                    catch
                    {
                        throw;
                    }

                    //各小组按比例给分
                    var A = Convert.ToInt32(groupNumber * classInfo.FivePointPercentage * 0.01);
                    var B = Convert.ToInt32(groupNumber * classInfo.FourPointPercentage * 0.01);
                    var C = Convert.ToInt32(groupNumber * classInfo.ThreePointPercentage * 0.01);
                    for (var i = 0; i < A; i++)
                    {
                        try
                        {
                            var seminarGroupTopic = _db.SeminarGroupTopic.SingleOrDefault(s => s.Id == idList[i]);
                            //如果找不到该组
                            if (seminarGroupTopic == null)
                            {
                                throw new GroupNotFoundException();
                            }

                            //更新报告分
                            seminarGroupTopic.PresentationGrade = 5;
                            _db.SaveChanges();
                        }
                        catch
                        {
                            throw;
                        }
                    }

                    for (var i = A; i < B; i++)
                    {
                        try
                        {
                            var seminarGroupTopic = _db.SeminarGroupTopic.SingleOrDefault(s => s.Id == idList[i]);
                            //如果找不到该组
                            if (seminarGroupTopic == null)
                            {
                                throw new GroupNotFoundException();
                            }

                            //更新报告分
                            seminarGroupTopic.PresentationGrade = 4;
                            _db.SaveChanges();
                        }
                        catch
                        {
                            throw;
                        }
                    }

                    for (var i = B; i < groupNumber; i++)
                    {
                        try
                        {
                            var seminarGroupTopic = _db.SeminarGroupTopic.SingleOrDefault(s => s.Id == idList[i]);
                            //如果找不到该组
                            if (seminarGroupTopic == null)
                            {
                                throw new GroupNotFoundException();
                            }

                            //更新报告分
                            seminarGroupTopic.PresentationGrade = 3;
                            _db.SaveChanges();
                        }
                        catch
                        {
                            throw;
                        }
                    }
                } //if end
            } //foreach topic end
        }
    }
}