using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserNetTest.Tools
{
    #region 系统接收信息类别
    public enum SYSMSG_TYPE
    {
        /// <summary>
        /// 扣费
        /// </summary>
        DEDUCT = 6,
        /// <summary>
        /// 强制下线
        /// </summary>
        TICKOFF = 7,
        /// <summary>
        /// 通知
        /// </summary>
        NOTIFY = 9,
        /// <summary>
        /// 关闭闲机
        /// </summary>
        IDLEOFF = 13,

    }

    #endregion
}
