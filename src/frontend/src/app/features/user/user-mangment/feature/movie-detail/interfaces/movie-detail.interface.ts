export interface Genre {
  genreId: number;
  name: string;
}

export interface CastMember {
  id: number;
  personName: string;
  imageUrl: string;
  roleType: string;
  characterName: string;
  displayOrder: number;
  isLead: boolean;
}

export interface MovieImage {
  id: number;
  imageUrl: string;
}

export interface MovieDetail {
  movieId: number;
  movieName: string;
  movieDescription: string;
  movieDuration: string;
  moviePoster: string;
  movieRating: number;
  movieAgeRating: string;
  language: string;
  genres: Genre[];
  trailerUrl?: string;
  showTimes: Showtime[];
  castMembers: CastMember[];
  images: MovieImage[];
}

export interface Showtime {
  movieShowTimeId: number;
  hallNumber: string;
  hallType: string;
  branchId: number;
  branchName: string;
  branchLocation: string;
  showStartTime: string;
  ticketPrice: number;
}

export interface ShowtimeGroup {
  date: string;
  formats: FormatGroup[];
}

export interface FormatGroup {
  format: string;
  branches: BranchGroup[];
}

export interface BranchGroup {
  branchName: string;
  branchLocation: string;
  halls: HallGroup[];
}

export interface HallGroup {
  hallNumber: string;
  hallType: string;
  showtimes: Showtime[];
}
