using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Test.Config
{
    public static class ProgramConstants
    {
        //Intent code for camera activity
        public static readonly int TAKE_PICTURE_CODE = 100;
        //Max Faces to detect in a picture
        public static readonly int MAX_FACES = 5;
    }
}