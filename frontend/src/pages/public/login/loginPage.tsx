import { useState } from "react";
import { Link } from "react-router-dom";
import { LogIn } from "lucide-react";
import styles from "./loginPage.module.css";
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
    <div className={styles.loginPage}>
      <div className={styles.loginBox}>
        <h1 className={styles.header}>Welcome Back!</h1>
        <p className={styles.text}>Sign in to continue creating and collaborating on playlists.</p>

        <form onSubmit={handleSubmit}>
          <div className={styles.formGroup}>
            <label>Username</label>
            <input
              name="username"
              value={values.username}
              onChange={handleChange}
              placeholder="Enter your username"
            />
            {errors.username && <p className={styles.error}>{errors.username}</p>}
          </div>

          <div className={styles.formGroup}>
            <label>Password</label>
            <input
              name="password"
              type="password"
              value={values.password}
              onChange={handleChange}
              placeholder="Enter your password"
            />
            {errors.password && <p className={styles.error}>{errors.password}</p>}
          </div>

          <button className={styles.signInBtn} type="submit">
            <LogIn className={styles.icon} strokeWidth={2} size={18} />
            Log In
          </button>
        </form>

        {message && <p className={styles.message}>{message}</p>}
        {errors.general && <p className={styles.error}>{errors.general}</p>}

        <p className={styles.signupText}>
          <span className={styles.grey}>Don’t have an account? </span>
          <Link to="/register">Sign up</Link>
        </p>
      </div>
    </div>
  );
};

export default LoginPage;
