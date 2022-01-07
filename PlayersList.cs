﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AntiAFk
{
    public class PlayersList
    {
        public int id;
        public RoleType role;
        public Vector3 Position, Rotation;
        public int timer = 0;

        public Camera079 camera;
        public float pitch, rot;
        public float energy, experience;
    }
}