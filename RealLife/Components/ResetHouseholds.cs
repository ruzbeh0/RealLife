using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace RealLife.Components
{
    public struct ResetHouseholds: IComponentData
    {
        public ResetType resetType;
    }

    public enum ResetType : int
    {
        FindNewHome = 1,
        Delete = 2
    }
}
