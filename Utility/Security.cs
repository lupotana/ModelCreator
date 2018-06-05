using System;
using System.Collections.Generic;
using System.Text;

namespace ModelCreator
{
    public class Security
    {
        private int executeProgram = 0;
        private int maxExecute = 0;

        private DateTime dateExpired = new DateTime(2011, 12, 31);
        public DateTime DateExpired
        {
            get { return dateExpired; }
        }

        public int ExecuteProgram
        {
            get { return executeProgram; }
            set { executeProgram = value; }
        }

        public bool CheckExpiration()
        {
            if (DateTime.Now.Year > dateExpired.Year) return false;
            else
            {
                if (DateTime.Now.Year == dateExpired.Year)
                {
                    if (DateTime.Now.Month > dateExpired.Month) return false;
                    else
                    {
                        if (DateTime.Now.Month == dateExpired.Month)
                        {
                            if (DateTime.Now.Day > dateExpired.Day) return false;
                            else
                            {
                                return true;
                            }
                        }
                        else return true;
                    }
                }
                else return true;
 
            }
        }

        public bool CheckExecute()
        {
            if (ExecuteProgram > maxExecute) return false;
            else return true;
        }
        


    }
}
