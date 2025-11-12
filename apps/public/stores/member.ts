import { defineStore } from 'pinia';
import { ref } from 'vue';

interface Member {
  id: number;
  name: string;
  bio: string;
  imageUrl: string;
}

export const useMemberStore = defineStore('member', () => {
  const members = ref<Member[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);

  // Placeholder for API call
  const fetchMembers = async () => {
    loading.value = true;
    error.value = null;
    try {
      // Simulate API call
      await new Promise(resolve => setTimeout(resolve, 1000));
      members.value = [
        { id: 1, name: 'Nguyễn Văn A', bio: 'Người sáng lập dòng họ, một nhân vật có tầm ảnh hưởng lớn trong cộng đồng.', imageUrl: 'https://via.placeholder.com/150/FFC107/FFFFFF?text=NVA' },
        { id: 2, name: 'Trần Thị B', bio: 'Nữ sĩ tài hoa với nhiều tác phẩm văn học để đời, được nhiều người kính trọng.', imageUrl: 'https://via.placeholder.com/150/4CAF50/FFFFFF?text=TTB' },
        { id: 3, name: 'Lê Văn C', bio: 'Anh hùng dân tộc, người đã có công lớn trong công cuộc bảo vệ quê hương.', imageUrl: 'https://via.placeholder.com/150/2196F3/FFFFFF?text=LVC' },
        { id: 4, name: 'Phạm Thị D', bio: 'Nhà giáo ưu tú, cống hiến cả đời cho sự nghiệp giáo dục.', imageUrl: 'https://via.placeholder.com/150/FF5722/FFFFFF?text=PTD' },
        { id: 5, name: 'Hoàng Văn E', bio: 'Kỹ sư tài năng, có nhiều đóng góp cho ngành công nghiệp.', imageUrl: 'https://via.placeholder.com/150/607D8B/FFFFFF?text=HVE' },
      ];
    } catch (e: any) {
      error.value = e.message || 'Failed to fetch members';
    } finally {
      loading.value = false;
    }
  };

  const getMemberById = (id: number) => {
    return members.value.find(member => member.id === id);
  };

  const searchMembers = (query: string) => {
    if (!query) return members.value;
    return members.value.filter(member =>
      member.name.toLowerCase().includes(query.toLowerCase())
    );
  };

  return {
    members,
    loading,
    error,
    fetchMembers,
    getMemberById,
    searchMembers,
  };
});
