using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LightCard
{
    public class Field 
    {
        public Space[,] spaces;

        public Field()
        {
            spaces = new Space[3,6];
        }
    }
}
