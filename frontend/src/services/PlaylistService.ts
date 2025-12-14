import api from "./api";

export const PlaylistService = {
  async getById(id: number) {
    const res = await api.get(`/api/playlists/${id}`);
    return res.data;
  },

  async getAll() {
    const res = await api.get(`/api/playlists`);
    return res.data;
  },

  async create(input: { name: string; description?: string; imageFile?: File }) {
    const token = localStorage.getItem("token");
    
    // Get current user to get hostId
    const userStr = localStorage.getItem("user");
    if (!userStr) throw new Error("Not authenticated");
    const currentUser = JSON.parse(userStr);
    
    const formData = new FormData();
    formData.append("name", input.name);
    formData.append("hostId", currentUser.id.toString()); // Send hostId
    if (input.description) formData.append("description", input.description);
    if (input.imageFile) formData.append("CoverImage", input.imageFile);
  
    const res = await api.post("/api/playlists", formData, {
      headers: {
        "Authorization": `Bearer ${token}`
      }
    });
  
    return res.data;
  },

 

  async update(id: number, data: Partial<any>) {
    const res = await api.patch(`/api/playlists/${id}`, data);
    return res.data;
  },

  async removeFromPlaylist(playlistId: number, songId: number) {
    await api.delete(`/api/playlists/${playlistId}/song/${songId}`);
  },

  async delete(id: number) {
    await api.delete(`/api/playlists/${id}`);
  },

  async addCollaborator(playlistId: number, username: string) {
    await api.post(`/api/playlists/${playlistId}/collaborators`, { username });
  },

  async removeCollaborator(playlistId: number, userId: number) {
    await api.delete(`/api/playlists/${playlistId}/collaborators/${userId}`);
  },

  async searchUsers(query: string) {
    const res = await api.get(`/api/users/search`, { params: { q: query } });
    return res.data ?? [];
  },
};
