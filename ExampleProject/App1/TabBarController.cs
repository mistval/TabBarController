/*
* MIT License
* 
* Copyright (c) 2019 Randall
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;

namespace Controls
{
    public class TabBarController : Fragment, BottomNavigationView.IOnNavigationItemSelectedListener
    {
        #region Fields

        private const string MENU_KEY = "menu";
        private const string NUMBER_OF_TABS_KEY = "number of tabs";
        private const string ACTIVE_TAB_INDEX_KEY = "active tab index";

        private static readonly int BOTTOM_NAVIGATION_ID = View.GenerateViewId();
        private static readonly int TAB_CONTENT_CONTAINER_ID = View.GenerateViewId();

        private readonly List<Fragment> tabFragments = new List<Fragment>();

        private int activeTabIndex;
        private IMenu menu;

        #endregion

        #region Properties

        public Fragment ActiveTabFragment
        {
            get
            {
                return this.tabFragments[this.activeTabIndex];
            }
        }

        #endregion

        #region Methods

        bool BottomNavigationView.IOnNavigationItemSelectedListener.OnNavigationItemSelected(IMenuItem item)
        {
            for (int i = 0; i < this.menu.Size(); ++i)
            {
                if (this.menu.GetItem(i) == item)
                {
                    this.UpdateActiveTab(i);
                    return true;
                }
            }

            return false;
        }

        public static TabBarController Create(int menuResource, IEnumerable<Fragment> fragments)
        {
            var args = new Bundle();
            args.PutInt(MENU_KEY, menuResource);
            var fragment = new TabBarController();
            fragment.Arguments = args;
            fragment.tabFragments.AddRange(fragments);
            fragment.activeTabIndex = 0;

            return fragment;
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutInt(NUMBER_OF_TABS_KEY, this.tabFragments.Count);
            outState.PutInt(ACTIVE_TAB_INDEX_KEY, this.activeTabIndex);

            for (int i = 0; i < this.tabFragments.Count; ++i)
            {
                this.ChildFragmentManager.PutFragment(
                    outState,
                    TabBarController.CreateFragmentKey(i),
                    this.tabFragments[i]
                );
            }
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (savedInstanceState != null)
            {
                this.activeTabIndex = savedInstanceState.GetInt(ACTIVE_TAB_INDEX_KEY);

                int numberOfTabs = savedInstanceState.GetInt(NUMBER_OF_TABS_KEY);
                for (int i = 0; i < numberOfTabs; ++i)
                {
                    this.tabFragments.Add(
                        this.ChildFragmentManager.GetFragment(savedInstanceState, TabBarController.CreateFragmentKey(i))
                    );
                }
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var bottomNavigation = TabBarController.CreateBottomNavigationView(this.Activity, this.Arguments);
            bottomNavigation.SetOnNavigationItemSelectedListener(this);
            this.menu = bottomNavigation.Menu;

            var fragmentContainer = TabBarController.CreateFragmentContainer(this.Activity);

            var relativeLayout = new RelativeLayout(this.Activity);
            relativeLayout.LayoutParameters = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent
            );

            relativeLayout.AddView(bottomNavigation);
            relativeLayout.AddView(fragmentContainer);

            if (savedInstanceState == null)
            {
                var transaction = this.ChildFragmentManager.BeginTransaction();

                for (int i = this.tabFragments.Count - 1; i >= 0; --i)
                {
                    transaction = transaction
                        .Add(TAB_CONTENT_CONTAINER_ID, this.tabFragments[i])
                        .Hide(this.tabFragments[i]);
                }

                transaction = transaction.Show(this.tabFragments[0]);
                transaction.Commit();
            }

            return relativeLayout;
        }

        private static LinearLayout CreateFragmentContainer(FragmentActivity activity)
        {
            var linearLayout = new LinearLayout(activity);
            var layoutParameters = new RelativeLayout.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent
            );

            layoutParameters.AddRule(LayoutRules.Above, BOTTOM_NAVIGATION_ID);

            linearLayout.LayoutParameters = layoutParameters;
            linearLayout.Id = TAB_CONTENT_CONTAINER_ID;

            return linearLayout;
        }

        private static BottomNavigationView CreateBottomNavigationView(FragmentActivity activity, Bundle arguments)
        {
            var bottomNavigation = new BottomNavigationView(activity);
            bottomNavigation.Id = BOTTOM_NAVIGATION_ID;
            var layoutParams = new RelativeLayout.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent
            );

            layoutParams.AddRule(LayoutRules.AlignParentBottom);
            bottomNavigation.LayoutParameters = layoutParams;

            var windowBackgroundValue = new TypedValue();
            activity.Theme.ResolveAttribute(Android.Resource.Attribute.WindowBackground, windowBackgroundValue, true);
            if (windowBackgroundValue.ResourceId != 0)
            {
                bottomNavigation.SetBackgroundResource(windowBackgroundValue.ResourceId);
            }
            else
            {
                bottomNavigation.SetBackgroundResource(windowBackgroundValue.Data);
            }

            bottomNavigation.InflateMenu(arguments.GetInt(MENU_KEY));
            return bottomNavigation;
        }

        private static string CreateFragmentKey(int index)
        {
            return $"fragment {index}";
        }

        private void UpdateActiveTab(int index)
        {
            this.ChildFragmentManager
                .BeginTransaction()
                .Hide(this.ActiveTabFragment)
                .Show(this.tabFragments[index])
                .Commit();

            this.activeTabIndex = index;
        }

        #endregion
    }
}