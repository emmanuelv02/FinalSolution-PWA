﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHttp.Application
{
    public abstract class ActionResult
    {
        public abstract void ExecuteResult(HttpContext context);
    }
}
