﻿// MvxActivity.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
//
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Views;
using MvvmCross.Platform.Droid.Views;

namespace MvvmCross.Droid.FullFragging.Views
{
    [Register("mvvmcross.droid.fullfragging.views.MvxActivity")]
    public abstract class MvxActivity
        : MvxEventSourceActivity
        , IMvxAndroidView
        , ViewTreeObserver.IOnGlobalLayoutListener
    {
        protected View _view;

        protected MvxActivity(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        protected MvxActivity()
        {
            BindingContext = new MvxAndroidBindingContext(this, this);
            this.AddEventListeners();
        }

        public object DataContext
        {
            get { return BindingContext.DataContext; }
            set { BindingContext.DataContext = value; }
        }

        public IMvxViewModel ViewModel
        {
            get
            {
                return DataContext as IMvxViewModel;
            }
            set
            {
                DataContext = value;
                OnViewModelSet();
            }
        }

        public void MvxInternalStartActivityForResult(Intent intent, int requestCode)
        {
            StartActivityForResult(intent, requestCode);
        }

        public IMvxBindingContext BindingContext { get; set; }

        public override void SetContentView(int layoutResId)
        {
            _view = this.BindingInflate(layoutResId, null);

            _view.ViewTreeObserver.AddOnGlobalLayoutListener(this);

            SetContentView(_view);
        }

        protected virtual void OnViewModelSet()
        {
        }

        protected override void AttachBaseContext(Context @base)
        {
            base.AttachBaseContext(MvxContextWrapper.Wrap(@base, this));
        }

        private readonly List<WeakReference<Fragment>> _fragList = new List<WeakReference<Fragment>>();

        public override void OnAttachFragment(Fragment fragment)
        {
            base.OnAttachFragment(fragment);
            _fragList.Add(new WeakReference<Fragment>(fragment));
        }

        public List<Fragment> Fragments
        {
            get
            {
                var ret = new List<Fragment>();
                foreach (var wref in _fragList)
                {
                    Fragment f;
                    if (!wref.TryGetTarget(out f)) continue;
                    if (f.IsVisible)
                        ret.Add(f);
                }

                return ret;
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            ViewModel?.ViewCreated();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ViewModel?.ViewDestroy();
        }

        protected override void OnStart()
        {
            base.OnStart();
            ViewModel?.ViewAppearing();
        }

        protected override void OnResume()
        {
            base.OnResume();
            ViewModel?.ViewAppeared();
        }

        protected override void OnPause()
        {
            base.OnPause();
            ViewModel?.ViewDisappearing();
        }

        protected override void OnStop()
        {
            base.OnStop();
            ViewModel?.ViewDisappeared();
        }

        public void OnGlobalLayout()
        {
            if (_view != null)
            {
                {
                    if (Build.VERSION.SdkInt < BuildVersionCodes.JellyBean)
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        _view.ViewTreeObserver.RemoveGlobalOnLayoutListener(this);
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                    else
                    {
                        _view.ViewTreeObserver.RemoveOnGlobalLayoutListener(this);
                    }
                }
                _view = null;
            }
        }
    }

    public abstract class MvxActivity<TViewModel>
        : MvxActivity
        , IMvxAndroidView<TViewModel> where TViewModel : class, IMvxViewModel
    {
        public new TViewModel ViewModel
        {
            get { return (TViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }
    }
}