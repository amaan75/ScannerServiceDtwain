using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dynarithmic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DTWAIN_SOURCE = System.IntPtr;
using DTWAIN_ARRAY = System.IntPtr;
using DTWAIN_RANGE = System.IntPtr;
using DTWAIN_FRAME = System.IntPtr;
using DTWAIN_PDFTEXTELEMENT = System.IntPtr;
using DTWAIN_HANDLE = System.IntPtr;
using DTWAIN_IDENTITY = System.IntPtr;
using DTWAIN_OCRENGINE = System.IntPtr;
using DTWAIN_OCRTEXTINFOHANDLE = System.IntPtr;
using TW_UINT16 = System.UInt16;
using TW_UINT32 = System.UInt32;
using TW_BOOL = System.UInt16;

namespace ScannerServiceDtwain.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            new Thread((state) => DoScan(state)).Start("TWAIN2 FreeImage Software Scanner");
            // new Thread((state) => DoScan(state)).Start("TWAIN2 FreeImage Software Scanner");
            // new Thread((state) => DoScan(state)).Start("WIA-Brother ADS-3600W LAN");


            return new List<WeatherForecast>();
        }

        private void DoScan(object obj)
        {
            try
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;
                Console.WriteLine("threadId" + threadId);
                string something = obj as string;
                DTWAIN_SOURCE src = IntPtr.Zero;
                Console.WriteLine("lib init start");
                TwainAPI.DTWAIN_SysInitialize();
                Console.WriteLine("lib init complete");
                DTWAIN_ARRAY sources = TwainAPI.DTWAIN_EnumSourcesEx();
                Something(sources);
                src = TwainAPI.DTWAIN_SelectSourceByName(something);
                TwainAPI.DTWAIN_OpenSource(src);
                int status = 0;
                DTWAIN_ARRAY array = TwainAPI.DTWAIN_CreateAcquisitionArray();
                TwainAPI.DTWAIN_EnableFeeder(src, 1);
                if (TwainAPI.DTWAIN_AcquireBufferedEx(src, TwainAPI.DTWAIN_PT_DEFAULT, 1, 0, 0, array, ref status) == 0)
                {
                    //todo failure
                    Console.WriteLine("acquireFailed");
                    return;
                }

                if (TwainAPI.DTWAIN_ArrayGetCount(array) == 0)
                {
                    //todo: no images found
                    Console.WriteLine("0 docs");
                    return;
                }

                DTWAIN_HANDLE dib = TwainAPI.DTWAIN_GetAcquiredImage(array, 0, 0);
                var image = Bitmap.FromHbitmap(TwainAPI.DTWAIN_ConvertDIBToBitmap(dib, System.IntPtr.Zero));
                image.Save(Guid.NewGuid() + something, ImageFormat.Jpeg);
                TwainAPI.DTWAIN_DestroyAcquisitionArray(array, 0);
                TwainAPI.DTWAIN_SysDestroy();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void Something(DTWAIN_ARRAY sources)
        {
            int nCount = TwainAPI.DTWAIN_ArrayGetCount(sources);
            if (nCount <= 0) return;


            // Display the sources

            DTWAIN_SOURCE currSource = IntPtr.Zero;

            for (int i = 0; i < nCount; ++i)
            {
                StringBuilder szName = new StringBuilder(256);
                TwainAPI.DTWAIN_ArrayGetSourceAt(sources, i, ref currSource);
                TwainAPI.DTWAIN_GetSourceProductName(currSource, szName, 255);
                Console.WriteLine(szName.ToString());
            }
        }
    }
}