import Home from './components/pages/home/Home';
import TermsOfService from './components/pages/legal/termsOfService';
import Profile from './components/pages/profile/Profile';
import EmailConfirmation from './components/pages/tokens/EmailConfirmation';
import ResetPassword from './components/pages/tokens/ResetPassword';
import Videos from './components/pages/video/Videos';

const AppRoutes = [
  {
    path:"*",
    index: true,
    element: (params) =>{
      return <Home {...params}/>
    },
  },
  {
    path: '/my-profile',
    element: (params) =>{
      return <Profile {...params}/>
    },
  },
  {
    path: '/confirmation',
    element: (params) =>{
      return <EmailConfirmation {...params}/>
    },
  },
  {
    path: '/reset-password',
    element: (params) =>{
      return <ResetPassword {...params}/>
    },
  },
  {
    path: '/all-videos',
    element: (params) =>{
      return <Videos {...params}/>
    },
  },
  {
    path: '/legal/terms-of-service',
    element: (params) =>{
      return <TermsOfService {...params}/>
    },
  },
];

export default AppRoutes;
