﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable All

namespace Tests.Sample
{
    namespace Complex
    {
        class Presentation
        {
            public class Web
            {
                public Business.Surrogate Surrogate;
            }

            public class Windows
            {

            }
            public class Mobile
            {

            }
            public class RIA
            {

            }

            public class UI
            {
                public class Control
                {
                    
                }
            }
        }

        class Business
        {
            public class Surrogate
            {
                private Operation operation;
            }
            
            class Operation
            {
                private Entity entity;
            }
            class Entity
            {
                private DataAcess.DataAcessIntegration integration;
            }
        }

        class DataAcess
        {
            public class DataAcessIntegration
            {
                private SqlDataBlock BlockA;
                private OracleDataBlock BlockB;
            }

            public class SqlDataBlock
            {

            }
            public class OracleDataBlock
            {

            }
            
        }
    }
}
