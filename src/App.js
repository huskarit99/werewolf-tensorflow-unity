import React, { Fragment } from "react";
import { BrowserRouter as Router, Switch, Route } from "react-router-dom";
import "./App.css";
import "./style.css";
import setAuthToken from "./utils/setAuthToken";

import Alert from "./components/Alert";

import AuthState from "./context/auth/AuthState";
import AlertState from "./context/alert/AlertState";

import Home from "./pages/Home";
import SignUp from "./pages/SignUp";
import Login from "./pages/Login";
import Logout from "./pages/Logout";

import PlayGame from "./pages/PlayGame";



import ResponsiveDrawer from "./layout/ResponsiveDrawer";
import { makeStyles } from "@material-ui/core/styles";
import io from "socket.io-client";

let socket;
const ENDPOINT = process.env.REACT_APP_API_URL;
socket = io(ENDPOINT, {
  transports: ["websocket", "polling", "flashsocket"],
});

export const ThemeContext = React.createContext("");

const useStyles = makeStyles((theme) => ({
  content: {
    padding: "2rem",
  },
}));

setAuthToken(localStorage.token);

const App = () => {
  const classes = useStyles();

  return (
    <Router>
      <AlertState>
        <AuthState>
          <Fragment>
            <ResponsiveDrawer>
              <ThemeContext.Provider value={socket}>
                <div className={classes.content}>
                  <Switch>
                    <Route exact path="/" component={Home} />
                    {/* Authenticate */}
                    <Route exact path="/login" component={Login} />
                    <Route exact path="/sign-up" component={SignUp} />
                    <Route exact path="/logout" component={Logout} />
                    {/*  */}
                  
                    <Route exact path="/room" component={PlayGame} />
                    
                  </Switch>
                </div>
              </ThemeContext.Provider>
            </ResponsiveDrawer>
            <Alert />
          </Fragment>
        </AuthState>
      </AlertState>
    </Router>
  );
};

export default App;
