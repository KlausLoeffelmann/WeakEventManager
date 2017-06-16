using System;
using System.ComponentModel;
using System.Reflection;

namespace WeakEventManager
{
    /// <summary>
    /// Allows to bind the PropertyChangedEvent from any ViewModel implementing INotifyPropertyChanged weak. 
    /// The type of the view does not matter.
    /// </summary>
    public class WeakEventManager
    {
        internal delegate void OIPropertyChangedEventHandler<viewType>(
                                                   viewType view,
                                                   object sender,
                                                   PropertyChangedEventArgs e);

        WeakReference myTargetObject;
        Delegate myOpenInstanceDelegate;

        /// <summary>
        /// Returns a WeakReference to a WeakPropertyChangedEventManager.
        /// </summary>
        /// <param name="viewModel">The ViewModel whose PropertyChanged Event we want to bind weakly.</param>
        /// <param name="targetProc">The target event handler, we want to bind weakly.</param>
        /// <returns></returns>
        public static WeakReference<WeakEventManager> AddHandlerWeak(
                                          INotifyPropertyChanged viewModel,
                                          PropertyChangedEventHandler targetProc)
        {
            return new WeakReference<WeakEventManager>(
                new WeakEventManager(viewModel,
                                     targetProc));
        }

        internal WeakEventManager(INotifyPropertyChanged viewModel,
                                Delegate targetProc)
        {
            object view = targetProc.Target;
            myTargetObject = new WeakReference(view);

            var delegateType = typeof(OIPropertyChangedEventHandler<>).MakeGenericType(view.GetType());
            
            myOpenInstanceDelegate = targetProc.GetMethodInfo().CreateDelegate(delegateType, null);

            (viewModel).PropertyChanged += PropertyChangedProc;
        }

        private void PropertyChangedProc(object sender, PropertyChangedEventArgs e)
        {
            //Re-Routing the Event.
            RaiseEvent(e);
        }

        private void RaiseEvent(PropertyChangedEventArgs e)
        {
            if (myOpenInstanceDelegate != null)
            {
                object tmpTarget;
                tmpTarget = myTargetObject.Target;
                if (tmpTarget != null)
                {
                    myOpenInstanceDelegate.DynamicInvoke(tmpTarget, this, e);
                }
            }
        }
    }
}
