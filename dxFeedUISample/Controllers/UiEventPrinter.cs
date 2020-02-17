using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using com.dxfeed.api;
using com.dxfeed.api.events;
using dxFeedUISample.Collections;

namespace dxFeedUISample.Controllers
{
    public class UiEventPrinter :
        IDxFeedListener,
        IDxTradeETHListener,
        IDxSpreadOrderListener,
        IDxCandleListener,
        IDxGreeksListener,
        IDxTheoPriceListener,
        IDxUnderlyingListener,
        IDxSeriesListener,
        IDxConfigurationListener,
        IDisposable
    {
        private const int LogSize = 30000;
        private readonly RangeObservableCollection<string> _log = new RangeObservableCollection<string>();

        public event EventHandler Printed;

        public void Print(string text)
        {
            Application.Current?.Dispatcher?.BeginInvoke(new Action(() =>
            {
                _log.Add(text);

                if (_log.Count > LogSize)
                {
                    _log.RemoveAt(_log.Count - 1);
                }

                Printed?.Invoke(this, EventArgs.Empty);
            }), DispatcherPriority.Render);
        }

        public void Print(IEnumerable<string> text)
        {
            Application.Current?.Dispatcher?.BeginInvoke(new Action(() =>
            {
                _log.AddRange(text);

                if (_log.Count > LogSize)
                {
                    _log.RemoveRange(_log.Count - LogSize);
                }

                Printed?.Invoke(this, EventArgs.Empty);
            }), DispatcherPriority.Render);
        }

        public ObservableCollection<string> GetLog()
        {
            return _log;
        }

        public void Print<TB, TE>(TB buf)
            where TB : IDxEventBuf<TE>
        {
            var dump = new List<string>();

            foreach (var record in buf)
            {
                dump.Add($"{buf.Symbol} {record}");
            }

            Print(dump);
        }

        #region Implementation of IDxFeedListener

        public void OnQuote<TB, TE>(TB buf)
            where TB : IDxEventBuf<TE>
            where TE : IDxQuote
        {
            Print<TB, TE>(buf);
        }

        public void OnTrade<TB, TE>(TB buf)
            where TB : IDxEventBuf<TE>
            where TE : IDxTrade
        {
            Print<TB, TE>(buf);
        }

        public void OnOrder<TB, TE>(TB buf)
            where TB : IDxEventBuf<TE>
            where TE : IDxOrder
        {
            Print<TB, TE>(buf);
        }

        public void OnProfile<TB, TE>(TB buf)
            where TB : IDxEventBuf<TE>
            where TE : IDxProfile
        {
            Print<TB, TE>(buf);
        }

        public void OnFundamental<TB, TE>(TB buf)
            where TB : IDxEventBuf<TE>
            where TE : IDxSummary
        {
            Print<TB, TE>(buf);
        }

        public void OnTimeAndSale<TB, TE>(TB buf)
            where TB : IDxEventBuf<TE>
            where TE : IDxTimeAndSale
        {
            Print<TB, TE>(buf);
        }

        #endregion

        #region Implementation of IDxTradeEthListener

        public void OnTradeETH<TB, TE>(TB buf)
            where TB : IDxEventBuf<TE>
            where TE : IDxTradeETH
        {
            Print<TB, TE>(buf);
        }

        #endregion

        #region Implementation of IDxSpreadOrderListener

        public void OnSpreadOrder<TB, TE>(TB buf)
            where TB : IDxEventBuf<TE>
            where TE : IDxSpreadOrder
        {
            Print<TB, TE>(buf);
        }

        #endregion

        #region Implementation of IDxCandleListener

        public void OnCandle<TB, TE>(TB buf)
            where TB : IDxEventBuf<TE>
            where TE : IDxCandle
        {
            Print<TB, TE>(buf);
        }

        #endregion

        #region Implementation of IDxGreeksListener

        public void OnGreeks<TB, TE>(TB buf)
            where TB : IDxEventBuf<TE>
            where TE : IDxGreeks
        {
            Print<TB, TE>(buf);
        }

        #endregion

        #region Implementation of IDxTheoPriceListener

        public void OnTheoPrice<TB, TE>(TB buf)
            where TB : IDxEventBuf<TE>
            where TE : IDxTheoPrice
        {
            Print<TB, TE>(buf);
        }

        #endregion

        #region Implementation of IDxUnderlyingListener

        public void OnUnderlying<TB, TE>(TB buf)
            where TB : IDxEventBuf<TE>
            where TE : IDxUnderlying
        {
            Print<TB, TE>(buf);
        }

        #endregion

        #region Implementation of IDxSeriesListener

        public void OnSeries<TB, TE>(TB buf)
            where TB : IDxEventBuf<TE>
            where TE : IDxSeries
        {
            Print<TB, TE>(buf);
        }

        #endregion

        #region Implementation of IDxConfigurationListener

        public void OnConfiguration<TB, TE>(TB buf)
            where TB : IDxEventBuf<TE>
            where TE : IDxConfiguration
        {
            Print<TB, TE>(buf);
        }

        #endregion

        public void Dispose()
        {
            _log.Clear();
        }
    }
}