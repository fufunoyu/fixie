﻿namespace Fixie.Internal
{
    using System;

    public class CasePassed : CaseCompleted
    {
        public CasePassed(Case @case, TimeSpan duration, string output)
            : base(@case, duration, output)
        {
        }
    }
}