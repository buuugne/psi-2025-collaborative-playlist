import { useState } from "react";
import { Link } from "react-router-dom";
import { LogIn } from "lucide-react";
import "./loginPage.scss";
import { useNavigate } from "react-router-dom";
import { authService } from "../../../services/authService";

const LoginPage = () => {
  const [values, setValues] = useState({ username: "", password: "" });
  const [errors, setErrors] = useState<any>({});
  const [message, setMessage] = useState("");

  const navigate = useNavigate();

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setValues((prev) => ({ ...prev, [name]: value }));
    setErrors((prev: any) => ({ ...prev, [name]: "" }));
  };

  const validate = () => {
    const newErrors: any = {}
    if (!values.username.trim()) newErrors.username = "Username is required.";
    if (!values.password) newErrors.password = "Password is required.";
    return newErrors;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    const validationErrors = validate();
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    try {
      setMessage("Logging in...");
      await authService.login(values.username, values.password);

      setMessage("Logged in successfully!");
      navigate("/home");
    } catch (err: any) {
      setMessage(""); 
      setErrors({ general: err.response?.data?.message || "Login failed" });
    }
  };

  return (
    <div className="login-page">
      <div className="login-page__box">
        <h1 className="login-page__header">Welcome Back!</h1>
        <p className="login-page__text">Sign in to continue creating and collaborating on playlists.</p>

        <form className="login-page__form" onSubmit={handleSubmit}>
          <div className="login-page__form-group">
            <label className="login-page__label">Username</label>
            <input
              className="login-page__input"
              name="username"
              value={values.username}
              onChange={handleChange}
              placeholder="Enter your username"
            />
            {errors.username && <p className="login-page__error">{errors.username}</p>}
          </div>

          <div className="login-page__form-group">
            <label className="login-page__label">Password</label>
            <input
              className="login-page__input"
              name="password"
              type="password"
              value={values.password}
              onChange={handleChange}
              placeholder="Enter your password"
            />
            {errors.password && <p className="login-page__error">{errors.password}</p>}
          </div>

          <button className="login-page__sign-in-btn" type="submit">
            <LogIn className="login-page__icon" strokeWidth={2} size={18} />
            Log In
          </button>
        </form>

        {message && <p className="login-page__message">{message}</p>}
        {errors.general && <p className="login-page__error">{errors.general}</p>}

        <p className="login-page__signup-text">
          <span className="login-page__grey">Don't have an account? </span>
          <Link to="/register">Sign up</Link>
        </p>
      </div>
    </div>
  );
};

export default LoginPage;