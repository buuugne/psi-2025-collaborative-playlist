import api from "./api";

export const UserService = {
  async updateProfileImage(userId: number, imageFile: File) {
    const formData = new FormData();
    formData.append('imageFile', imageFile);

    const res = await api.put(
      `/api/users/${userId}/profile-image`,
      formData
    );

    return res.data;
  },

  async getCurrentUser() {
    const res = await api.get(`/api/users/me`);
    return res.data;
  }
};