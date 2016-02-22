﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MrGibbs.Models;

namespace MrGibbs.Contracts
{
    public interface ICalculator : IPluginComponent
    {
        void Calculate(State state);
    }
}