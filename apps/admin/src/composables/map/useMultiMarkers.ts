import { ref, watch, onMounted, type Ref } from 'vue';
import type * as mapboxgl from 'mapbox-gl';
import { LocationType } from '@/types/familyLocation.d';
import { defaultMapboxMarkerAdapter, defaultMapboxBoundsAdapter, type IMapboxMarkerAdapter, type IMapboxBoundsAdapter } from '@/composables/utils/mapbox.adapter';
import type { Marker } from 'mapbox-gl'; // Import Marker type directly from mapbox-gl
import { getLocationTypeIcon } from '@/composables/utils/map.utils';

export interface MapMarker {
  id: string;
  lng: number;
  lat: number;
  title?: string;
  description?: string;
  address?: string; // Add address field
  locationType: LocationType;
}

interface UseMultiMarkersOptions {
  mapInstance: Ref<mapboxgl.Map | null>;
  markers: Ref<MapMarker[] | undefined>;
  initialZoom?: number;
  mapboxMarkerAdapter?: IMapboxMarkerAdapter;
  mapboxBoundsAdapter?: IMapboxBoundsAdapter;
}

export function useMultiMarkers(options: UseMultiMarkersOptions) {
  const { mapboxMarkerAdapter = defaultMapboxMarkerAdapter, mapboxBoundsAdapter = defaultMapboxBoundsAdapter } = options;
  const activeMarkers = ref<Marker[]>([]);

  const fitMapToMarkers = (markers: MapMarker[]) => {
    if (!options.mapInstance.value || markers.length === 0) {
      return;
    }
    const bounds = mapboxBoundsAdapter.createBounds();
    markers.forEach(marker => {
      mapboxBoundsAdapter.extendBounds(bounds, [marker.lng, marker.lat]);
    });
    mapboxBoundsAdapter.fitMapBounds(options.mapInstance.value, bounds, {
      maxZoom: options.initialZoom || 10,
    });
  };

  const addMarkersToMap = (markers: MapMarker[]) => {
    if (!options.mapInstance.value) return;

    activeMarkers.value = markers.map(loc => {
      if (isNaN(loc.lng!) || isNaN(loc.lat!)) {
        console.warn(`Skipping marker with invalid coordinates: ${loc.id}, Lng: ${loc.lng}, Lat: ${loc.lat}`);
        return null;
      }

      let markerColor = 'gray';

      switch (loc.locationType) {
        case LocationType.Grave:
          markerColor = '#4CAF50';
          break;
        case LocationType.Homeland:
          markerColor = '#2196F3';
          break;
        case LocationType.AncestralHall:
          markerColor = '#FFC107';
          break;
        case LocationType.Cemetery:
          markerColor = '#9C27B0';
          break;
        case LocationType.EventLocation:
          markerColor = '#FF5722';
          break;
        case LocationType.Other:
          markerColor = '#607D8B';
          break;
      }

      const marker = mapboxMarkerAdapter.createMarker({
        color: markerColor,
        anchor: 'bottom'
      });
      if (options.mapInstance.value) {
        mapboxMarkerAdapter.setMarkerLngLat(marker, [loc.lng!, loc.lat!]);
        mapboxMarkerAdapter.addMarkerToMap(marker, options.mapInstance.value);
      }
      if (loc.title || loc.description || loc.address || loc.lng || loc.lat) {
        const popupContent = `
          <div style="font-family: Arial, sans-serif; padding: 10px; max-width: 250px;">
            <div style="display: flex; align-items: center; margin-bottom: 5px;">
              <i class="mdi ${getLocationTypeIcon(loc.locationType)}" style="font-size: 20px; color: #424242; margin-right: 8px;"></i>
              <strong style="font-size: 16px; color: #333333;">${loc.title || 'Địa điểm không tên'}</strong>
            </div>
            ${loc.address ? `<p style="font-size: 13px; color: #555555; margin-bottom: 5px;">Địa chỉ: ${loc.address}</p>` : ''}
            ${loc.description ? `<p style="font-size: 13px; color: #555555; margin-bottom: 5px;">Mô tả: ${loc.description}</p>` : ''}
            <p style="font-size: 12px; color: #777777; margin-top: 5px;">Kinh độ: ${loc.lng?.toFixed(4)}, Vĩ độ: ${loc.lat?.toFixed(4)}</p>
          </div>
        `;
        mapboxMarkerAdapter.setMarkerPopup(marker, mapboxMarkerAdapter.setPopupHtml(mapboxMarkerAdapter.createPopup(), popupContent));
      }

      return marker;
    }).filter(Boolean) as mapboxgl.Marker[];
  };

  const handleMarkersChange = (newMarkers: MapMarker[] | undefined) => {
    // Remove existing markers
    activeMarkers.value.forEach(marker => mapboxMarkerAdapter.removeMarker(marker as mapboxgl.Marker));
    activeMarkers.value = [];
    // Add new markers
    const markersToAdd = newMarkers || [];
    addMarkersToMap(markersToAdd);
    // Fit map to new markers
    if (markersToAdd.length > 0) {
      if (options.mapInstance.value && options.mapInstance.value.isStyleLoaded && options.mapInstance.value.isStyleLoaded()) {
        fitMapToMarkers(markersToAdd);
      } else if (options.mapInstance.value) {
        // If map is not loaded yet, fit after load
        options.mapInstance.value.on('load', () => {
          fitMapToMarkers(markersToAdd);
        });
      }
    }
  };

  const handleMapMounted = () => {
    if (options.mapInstance.value && (options.markers.value?.length || 0) > 0) {
      // Initial load, ensure markers are added and map fitted
      options.mapInstance.value.on('load', () => {
        addMarkersToMap(options.markers.value || []);
        fitMapToMarkers(options.markers.value || []);
      });
    }
  };

  watch(() => options.markers.value, handleMarkersChange, { deep: true, immediate: true });

  onMounted(handleMapMounted);

  return {
    state: {
      activeMarkers,
    },
    actions: {
      addMarkersToMap,
      fitMapToMarkers,
    },
  };
}
