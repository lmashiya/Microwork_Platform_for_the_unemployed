using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Microwork_Platform_for_the_unemployed
{
    public static class Speaker
    {

        public static void Speak(string message)
        {
            using (SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer())
            {

                SpeechSynthesizer reader = new SpeechSynthesizer
                {
                    Rate = 0,
                    Volume = 100
                };
                reader.SpeakAsync(message);
            }
        }
    }
}