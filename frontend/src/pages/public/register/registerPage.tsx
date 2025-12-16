import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { UserPlus } from "lucide-react";
import "./registerPage.scss";
import { authService } from "../../../services/authService";

const RegisterPage = () => {
  const [values, setValues] = useState({
    username: "",
    password: "",
    confirmPassword: "",
  });
  const [errors, setErrors] = useState<any>({});
  const [message, setMessage] = useState("");
  const navigate = useNavigate();

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setValues((prev) => ({ ...prev, [name]: value }));
    setErrors((prev: any) => ({ ...prev, [name]: "" }));
  };

  const validate = () => {
    const newErrors: any = {};
    if (!values.username.trim()) 
      newErrors.username = "Username is required.";  
    if (!values.password || values.password.length < 8)  
      newErrors.password = "Password must be at least 8 characters.";
    if (values.password !== values.confirmPassword)
      newErrors.confirmPassword = "Passwords do not match.";
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
      setMessage("Registering...");
      await authService.register(values.username, values.password, values.confirmPassword);

      setMessage("Account created successfully!");
      navigate("/home");  
    } catch (err: any) {
      setMessage("");
      setErrors({ general: err.response?.data?.message || "Registration failed" });
    }
  };

  return (
    <div className="register-page">
      <div className="register-page__box">
        <h1 className="register-page__header">Create your account</h1>
        <p className="register-page__text">Start building collaborative playlists in seconds.</p>

        <form className="register-page__form" onSubmit={handleSubmit}>
          <div className="register-page__form-group">
            <label className="register-page__label">Username</label>
            <input
              className="register-page__input"
              name="username"
              value={values.username}
              onChange={handleChange}
              placeholder="Enter your username"
            />
            {errors.username && <p className="register-page__error">{errors.username}</p>}
          </div>

          <div className="register-page__form-group">
            <label className="register-page__label">Password</label>
            <input
              className="register-page__input"
              name="password"
              type="password"
              value={values.password}
              onChange={handleChange}
              placeholder="Enter your password"
            />
            {errors.password && <p className="register-page__error">{errors.password}</p>}
          </div>

          <div className="register-page__form-group">
            <label className="register-page__label">Confirm Password</label>
            <input
              className="register-page__input"
              name="confirmPassword"
              type="password"
              value={values.confirmPassword}
              onChange={handleChange}
              placeholder="Confirm your password"
            />
            {errors.confirmPassword && <p className="register-page__error">{errors.confirmPassword}</p>}
          </div>

          <button className="register-page__sign-up-btn" type="submit">
            <UserPlus className="register-page__icon" size={18} />
            Sign Up
          </button>
        </form>

        {message && <p className="register-page__message">{message}</p>}
        {errors.general && <p className="register-page__error">{errors.general}</p>}

        <p className="register-page__login-text">
          <span className="register-page__grey">Already have an account? </span>
          <Link to="/login">Log in</Link>
        </p>
      </div>
    </div>
  );
};

export default RegisterPage;