import React from 'react';
import ReactDOM from 'react-dom/client';
import { AuthProvider } from "react-oidc-context";
import { User } from "oidc-client-ts";
import './index.css';
import App from './App';
import reportWebVitals from './reportWebVitals';

const signinCallback = (_user: User | void): void => {
  window.history.replaceState(
      {},
      document.title,
      window.location.pathname
  )
};

const oidcConfig = {
  authority: "https://localhost:5001",
  client_id: "spa-user-ui",
  redirect_uri: "https://localhost:3000/callback.html",
  scope: "openid profile read:notes list:notes",
  onSigninCallback: signinCallback,
};

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
);
root.render(
  <React.StrictMode>
    <AuthProvider {...oidcConfig}>
      <App />
    </AuthProvider>
  </React.StrictMode>
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
