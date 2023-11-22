import { useAuth } from "react-oidc-context";
import { UserManager, User } from "oidc-client-ts";
import logo from './logo.svg';
import './App.css';
import Notes from "./Notes";

function getUser() {
  const oidcStorage = sessionStorage.getItem('oidc.user:https://localhost:5001:spa-user-ui')
  if (!oidcStorage) {
    return null;
  }

  return User.fromStorageString(oidcStorage);
}

function App() {
  const auth = useAuth();

  switch (auth.activeNavigator) {
    case "signinSilent":
      return <div>Signing you in...</div>;
    case "signoutRedirect":
      return <div>Signing you out...</div>;
  }

  if (auth.isLoading) {
    return <div>Loading...</div>;
  }

  if (auth.error) {
    return <div>Oops... {auth.error.message}</div>;
  }

  if (auth.isAuthenticated) {
    const user = getUser();
    const token = user?.access_token;

    return (
      <div className="App">
        <header className="App-header">
          Hello {auth.user?.profile.name}{" "}
          <button onClick={() => void auth.removeUser()}>Log out</button>
          <div>
            Your access token is:
            <div>
              <textarea className="token-display" defaultValue={token} />
            </div>
          </div>

          <Notes />
        </header>
      </div>
    );
  }

  return (
    <div className="App">
      <header className="App-header">
        <button onClick={() => void auth.signinRedirect()}>Log in</button>
      </header>
    </div>
  );
}

export default App;
