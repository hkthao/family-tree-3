from abc import ABC, abstractmethod
from typing import List, Dict, Any, Optional

class IFaceRepository(ABC):
    """
    Abstract Base Class for face data storage and retrieval.
    Defines the contract for any class that provides persistent storage for face vectors and metadata.
    """

    @abstractmethod
    async def upsert_face_vector(self, face_id: str, vector: List[float], metadata: Dict[str, Any]):
        """
        Inserts or updates a face vector and its associated metadata in the repository.

        Args:
            face_id (str): A unique identifier for the face.
            vector (List[float]): The embedding vector of the face.
            metadata (Dict[str, Any]): A dictionary containing metadata for the face.
        """
        pass

    @abstractmethod
    async def search_similar_faces(
        self,
        query_vector: List[float],
        family_id: Optional[str] = None,
        member_id: Optional[str] = None,
        top_k: int = 5,
        threshold: float = 0.75,
    ) -> List[Dict[str, Any]]:
        """
        Searches for similar faces based on a query vector.

        Args:
            query_vector (List[float]): The embedding vector to search with.
            family_id (Optional[str]): Filters search results by family ID.
            member_id (Optional[str]): Filters search results by member ID.
            top_k (int): The maximum number of similar faces to return.
            threshold (float): The similarity threshold.

        Returns:
            List[Dict[str, Any]]: A list of dictionaries, where each dictionary represents a
                                  found face with its score and payload (metadata).
        """
        pass

    @abstractmethod
    async def get_faces_by_family_id(self, family_id: str) -> List[Dict[str, Any]]:
        """
        Retrieves all faces associated with a given family ID.

        Args:
            family_id (str): The ID of the family.

        Returns:
            List[Dict[str, Any]]: A list of dictionaries, each representing a face
                                  with its ID and metadata.
        """
        pass

    @abstractmethod
    async def delete_face(self, face_id: str) -> bool:
        """
        Deletes a specific face by its ID.

        Args:
            face_id (str): The unique identifier of the face to delete.

        Returns:
            bool: True if the face was successfully deleted, False otherwise.
        """
        pass

    @abstractmethod
    async def delete_faces_by_family_id(self, family_id: str) -> bool:
        """
        Deletes all faces associated with a given family ID.

        Args:
            family_id (str): The ID of the family whose faces are to be deleted.

        Returns:
            bool: True if faces were successfully deleted, False otherwise.
        """
        pass

    @abstractmethod
    async def delete_faces_by_member_id(self, member_id: str) -> bool:
        """
        Deletes all faces associated with a given member ID.

        Args:
            member_id (str): The ID of the member whose faces are to be deleted.

        Returns:
            bool: True if faces were successfully deleted, False otherwise.
        """
        pass
