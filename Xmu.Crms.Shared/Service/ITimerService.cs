﻿using System;
using System.Threading.Tasks;

namespace Xmu.Crms.Shared.Service
{
    /// <summary>
    ///     定时器
    ///     @author qinlingyun liuaiqi
    ///     @version 2.00
    /// </summary>
    public interface ITimerService : Orleans.IGrainWithIntegerKey
    {
        /// <summary>
        ///     向Event表插入数据.
        ///     @author qinlingyun
        /// </summary>
        /// <param name="time">事件的时间</param>
        /// <param name="bean">方法名称</param>
        /// <param name="parameter">方法参数</param>
        Task InsertEvent(DateTime time, string bean, string parameter);

        /// <summary>
        ///     更新Event表.
        ///     @author qinlingyun
        /// </summary>
        /// <param name="time">事件的时间</param>
        /// <param name="bean">方法名称</param>
        /// <param name="parameter">方法参数</param>
        Task UpdateEvent(DateTime time, string bean, string parameter);


        /// <summary>
        ///     每十分钟检查一次Event实体的状况
        ///     @author qinlingyun
        /// </summary>
        Task Scheduled();
    }
}