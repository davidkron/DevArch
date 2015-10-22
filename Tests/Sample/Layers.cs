﻿// ReSharper disable All

namespace Tests.Sample
{
    namespace Layers
    {
        class Domain
        {
            public class Entity { }
        }
        class DataAccess
        {
            Domain.Entity entity;
        }
        class Logic
        {
            DataAccess access;
            Domain.Entity entity;
        }
        class Presentation
        {
            Logic logic;
        }
    }
}
