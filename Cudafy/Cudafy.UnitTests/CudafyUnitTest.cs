﻿/*
CUDAfy.NET - LGPL 2.1 License
Please consider purchasing a commerical license - it helps development, frees you from LGPL restrictions
and provides you with support.  Thank you!
Copyright (C) 2011 Hybrid DSP Systems
http://www.hybriddsp.com

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using NUnit.Framework;
using Cudafy;

namespace Cudafy.UnitTests
{
    public class CudafyUnitTest
    {
        protected bool CompareEx<T>(Array exp, Array act, int expOffset = 0, int actOffset = 0, int n = 0)
        {
            //if (exp.Length != act.Length)
            //    return false;
            List<T> expList = exp.Cast<T>().ToList();
            List<T> actList = act.Cast<T>().ToList();
            int len = (n == 0 ? Math.Min(act.Length, exp.Length) : n);
            for (int i = 0; i < len; i++)
            {
                object expO = expList[expOffset + i];
                object actO = actList[actOffset + i];
                //Debug.WriteLine(string.Format("{0}: {1} {2}", i, actO, expO));
                if (!expO.Equals(actO))
                    return false;
            }
            return true;
        }
        
        protected bool Compare<T>(T[] exp, T[] act, int expOffset = 0, int actOffset = 0, int n = 0)
        {
            if (exp.Length != act.Length)
                return false;
            for (int i = 0; i < (n == 0 ? act.Length : n); i++)
            {
                if (!exp[expOffset + i].Equals(act[actOffset + i]))
                    return false;
            }
            return true;
        }

        protected bool Compare<T>(T exp, T[] act, int actOffset = 0, int n = 0)
        {

            for (int i = 0; i < (n == 0 ? act.Length : n); i++)
            {
                if (!exp.Equals(act[actOffset + i]))
                    return false;
            }
            return true;
        }
        
        public static void PerformAllTests(ICudafyUnitTest test)
        {
            MethodInfo[] miArray = test.GetType().GetMethods();
            int ctr = 0;
            MethodInfo setup = test.GetType().GetMethod("SetUp");
            MethodInfo tearDown = test.GetType().GetMethod("TearDown");
            MethodInfo testSetup = test.GetType().GetMethod("TestSetUp");
            MethodInfo testTearDown = test.GetType().GetMethod("TestTearDown");
            List<MethodInfo> miList = miArray.ToList();
            if (setup != null)
                setup.Invoke(test, null);
            foreach (MethodInfo mi in miList.OrderBy(mi => mi.Name))
            {

                try
                {
                    bool isTest = false;
                    foreach (object o in Attribute.GetCustomAttributes(mi))
                        if (o is TestAttribute)
                            isTest = true;
                    if (!isTest)
                    {
                        continue;
                    }
                    if (mi.GetParameters().Length == 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine(string.Format("{0}: {1}", ctr++, mi.Name));
                        if (testSetup != null)
                            testSetup.Invoke(test, null);
                        mi.Invoke(test, null);
                        if (testTearDown != null)
                            testTearDown.Invoke(test, null);
                    }
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException is CudafyException)
                        Console.WriteLine(ex.InnerException.GetType().Name + ": " + ex.InnerException.Message);
                    else
                        throw ex;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            if (tearDown != null)
                tearDown.Invoke(test, null);
        }
    }
}