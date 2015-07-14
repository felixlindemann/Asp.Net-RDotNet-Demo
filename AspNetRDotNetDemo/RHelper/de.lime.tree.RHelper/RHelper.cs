using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using RDotNet;

namespace AspNetRDotNetDemo.de.lime.tree.RHelper
{

    public class RHelper : IDisposable
    {

        #region Variables

        private REngine rEngine;

        private int sinkwidth = 1000;

        private String rHome = @"c:\Program Files\R\R-3.1.2";

        private Boolean is64Bit = Environment.Is64BitProcess;

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public int SinkWidth
        {
            get { return sinkwidth; }
            set
            {
                sinkwidth = value;

                if (this.rEngine != null)
                {
                    this.rEngine.Evaluate(string.Format("options(width = {0})", sinkwidth));

                }
            }
        }

        /// <summary>
        /// The R-Home Directory 
        /// </summary>
        public String RHome
        {
            get { return rHome; }
            set
            {
                rHome = value;
            }
        }

        /// <summary>
        /// R-home as DirectoryInfo
        /// </summary>
        private DirectoryInfo dirRHome
        {
            get
            {
                return new DirectoryInfo(rHome);
            }
        }

        /// <summary>
        /// DirectoryInfo of Binaries
        /// </summary>
        private DirectoryInfo dirRBin
        {
            get
            {
                return new DirectoryInfo(Path.Combine(dirRHome.FullName, "bin\\" + (is64Bit ? "x64" : "i386")));
            }
        }

        /// <summary>
        /// not tested if 32bit Application can run a 64bit R instance and vice Versa.
        /// </summary>
        public Boolean Is64Bit
        {
            get { return is64Bit; }
            set { is64Bit = value; }
        }


        #endregion

        #region Constructor

        /// <summary>
        /// Constructor Method
        /// 
        /// Checks for Checking existing and choosing R 
        /// x64/i386
        /// </summary>
        /// <param name="autoprint">is needed for debugging purposes</param>
        public RHelper(bool autoprint = true)
        {
            if (dirRHome.Exists)
            {
                if (dirRBin.Exists)
                {

                    REngine.SetEnvironmentVariables(dirRBin.FullName, dirRHome.FullName);
                    this.rEngine = REngine.GetInstance();

                    // Use for Debug Purpose.
                    this.rEngine.AutoPrint = autoprint;
                    // Update sinkwidt in rEngine instance
                    this.SinkWidth = this.sinkwidth;
                    return;
                }
            }
            // throw Exceptions, if R Could not be found.
            if (is64Bit) throw new SystemException("an installation of R 64bit is required.");
            throw new SystemException("an installation of R 32bit is required.");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Locale function for executing to avoid loop
        /// </summary>
        /// <param name="cmd">the line to be executed.</param>
        /// <returns></returns>
        public SymbolicExpression Evaluate(String cmd)
        {
            try
            {
                // Evaluate Command and add Result to result list.
                return rEngine.Evaluate(cmd);

            }
            catch (Exception)
            {
                throw;
            }

        }

        #endregion

        #region get variables from R

        public int getInt(string cmd)
        {
            return this.getIntegerVector(cmd)[0];
        }

        public String getString(string cmd)
        {
            return this.getCharacterVector(cmd)[0];
        }

        public IntegerVector getIntegerVector(string cmd)
        {
            return this.rEngine.Evaluate(cmd).AsInteger();
        }
        public NumericVector getNumericVector(string cmd)
        {
            return this.rEngine.Evaluate(cmd).AsNumeric();
        }

        public CharacterVector getCharacterVector(string cmd)
        {
            return this.rEngine.Evaluate(cmd).AsCharacter();
        }

        public GenericVector getGenericVector(string cmd)
        {
            return this.rEngine.Evaluate(cmd).AsList();
        }

        #endregion

        #region Transform


        #endregion


        #region Dispose

        public void Dispose()
        {
            if (this.rEngine != null)
            {
                try
                {
                    // if any plots are open, they have to be closed 
                    // before shooting down R
                    int I = this.getInt("length(dev.list())");
                    for (int i = 0; i < I; i++)
                    {
                        this.Evaluate("dev.off()");
                    }
                    // Shoot down
                    this.rEngine.Dispose();
                    this.rEngine = null;
                }
                catch (Exception ex)
                {

                    throw ex;
                }

            }

        }

        #endregion
    }
}
